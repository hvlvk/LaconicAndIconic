using LaconicAndIconic.BLL;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace LaconicAndIconic.Tests.Services;

public class RecipeServiceTests : IDisposable
{
    private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IRepository<Rating>> _ratingRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationServiceMock;
    private readonly MemoryCache _memoryCache;
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _recipeRepositoryMock = new Mock<IRecipeRepository>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _ratingRepositoryMock = new Mock<IRepository<Rating>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _cacheInvalidationServiceMock = new Mock<ICacheInvalidationService>();
        var fileServiceMock = new Mock<IFileService>();
        var cachingOptions = Options.Create(new CachingOptions());

        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _service = new RecipeService(
            _recipeRepositoryMock.Object,
            fileServiceMock.Object,
            _categoryRepositoryMock.Object,
            _ratingRepositoryMock.Object,
            _userRepositoryMock.Object,
            _memoryCache,
            _cacheInvalidationServiceMock.Object,
            cachingOptions);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryCache?.Dispose();
        }
    }

    [Fact]
    public async Task CreateRecipeAsync_ValidData_ReturnsSuccess()
    {
        var dto = new CreateRecipeDto
        {
            Title = "Test Recipe",
            Description = "A delicious test recipe",
            PrepTimeMin = 30,
            Servings = 4,
            Ingredients = "Eggs\nMilk",
            CookingMethod = "Mix\nBake",
            CategoryId = 1
        };

        _categoryRepositoryMock.Setup(repo => repo.ExistsAsync(1)).ReturnsAsync(true);
        _recipeRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Recipe>())).Returns(Task.CompletedTask);
        _recipeRepositoryMock.Setup(repo => repo.GetByIdAsync(0)).ReturnsAsync(new Recipe { Id = 1, Title = "Test Recipe", CategoryId = 1, AuthorId = 1, PrepTimeMin = 30, Description = "A delicious test recipe" });

        var result = await _service.CreateRecipeAsync(1, dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test Recipe", result.Value.Title);
        _recipeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Recipe>()), Times.Once);
        _recipeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRecipeAsync_MissingTitle_ReturnsFailure()
    {
        var dto = new CreateRecipeDto { Title = "  " };

        var result = await _service.CreateRecipeAsync(1, dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Назва обов'язкова", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateRecipeAsync_CategoryNotFound_ReturnsFailure()
    {
        var dto = new CreateRecipeDto
        {
            Title = "Title",
            Description = "Desc",
            PrepTimeMin = 10,
            Servings = 2,
            Ingredients = "Item",
            CookingMethod = "Step",
            CategoryId = 99
        };
        _categoryRepositoryMock.Setup(repo => repo.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.CreateRecipeAsync(1, dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Категорія не знайдена", result.ErrorMessage);
    }

    [Fact]
    public async Task GetRecipesByAuthorIdAsync_ReturnsRecipes()
    {
        var authorId = 1;
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "R1",
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow,
                Category = new Category { Name = "Dinner" },
                Author = new ApplicationUser { UserName = "chef1" }
            },
            new Recipe
            {
                Id = 2,
                Title = "R2",
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                Category = new Category { Name = "Dessert" },
                Author = new ApplicationUser { UserName = "chef2" }
            }
        };

        _recipeRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync(recipes);

        var result = await _service.GetRecipesByAuthorIdAsync(authorId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Equal("R1", result.Value!.First().Title);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_RecipeExists_ReturnsMappedDto()
    {
        var recipe = new Recipe
        {
            Id = 5,
            Title = "Pasta",
            Description = "Tasty",
            PrepTimeMin = 20,
            Servings = 2,
            Ingredients = "Tomato\nPasta",
            CookingMethod = "Boil\nServe",
            CategoryId = 2,
            AuthorId = 3,
            Category = new Category { Name = "Italian" },
            Author = new ApplicationUser { UserName = "chef" }
        };

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync([recipe]);

        var result = await _service.GetRecipeByIdAsync(5);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Pasta", result.Value!.Title);
        Assert.Equal("Italian", result.Value.CategoryName);
        Assert.Equal("chef", result.Value.AuthorName);
        Assert.Equal(2, result.Value.Servings);
        Assert.Equal("Tomato\nPasta", result.Value.Ingredients);
        Assert.Equal("Boil\nServe", result.Value.CookingMethod);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_RecipeExists_ReturnsRatingSummary()
    {
        var recipe = new Recipe
        {
            Id = 6,
            Title = "Soup",
            Description = "Warm",
            PrepTimeMin = 30,
            Servings = 4,
            Ingredients = "Water\nVeggies",
            CookingMethod = "Boil",
            CategoryId = 2,
            AuthorId = 3,
            Category = new Category { Name = "Dinner" },
            Author = new ApplicationUser { UserName = "chef" }
        };

        var currentUserRating = new Rating
        {
            RecipeId = 6,
            UserId = 10,
            Score = 5,
            Recipe = recipe,
            User = new ApplicationUser { Id = 10, UserName = "rater" }
        };

        recipe.Ratings.Add(currentUserRating);
        recipe.Ratings.Add(new Rating
        {
            RecipeId = 6,
            UserId = 11,
            Score = 3,
            Recipe = recipe,
            User = new ApplicationUser { Id = 11, UserName = "rater2" }
        });

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>() ))
            .ReturnsAsync([recipe]);

        var result = await _service.GetRecipeByIdAsync(6, 10);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(4.0, result.Value!.AverageRating);
        Assert.Equal(2, result.Value.RatingCount);
        Assert.Equal(5, result.Value.CurrentUserRating);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_RecipeDoesNotExist_ReturnsFailure()
    {
        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync([]);

        var result = await _service.GetRecipeByIdAsync(404);

        Assert.False(result.IsSuccess);
        Assert.Equal("Рецепт не знайдено", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteRecipeAsync_ExistingRecipeAndValidAuthor_ReturnsSuccess()
    {
        var recipeId = 1;
        var authorId = 1;
        var recipe = new Recipe { Id = recipeId, AuthorId = authorId };

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(recipeId)).ReturnsAsync(recipe);

        var result = await _service.DeleteRecipeAsync(recipeId, authorId);

        Assert.True(result.IsSuccess);
        _recipeRepositoryMock.Verify(r => r.Remove(recipe), Times.Once);
        _recipeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RecipeNotFound_ReturnsFailure()
    {
        var recipeId = 99;
        var authorId = 1;

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(recipeId)).ReturnsAsync((Recipe?)null);

        var result = await _service.DeleteRecipeAsync(recipeId, authorId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Рецепт не знайдено", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteRecipeAsync_WrongAuthor_ReturnsFailure()
    {
        var recipeId = 1;
        var authorId = 2;
        var recipe = new Recipe { Id = recipeId, AuthorId = 1 };

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(recipeId)).ReturnsAsync(recipe);

        var result = await _service.DeleteRecipeAsync(recipeId, authorId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви можете видаляти тільки свої рецепти", result.ErrorMessage);
        _recipeRepositoryMock.Verify(r => r.Remove(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task RateRecipeAsync_NewRating_AddsRating()
    {
        var recipe = new Recipe
        {
            Id = 20,
            Title = "Pizza",
            CategoryId = 1,
            AuthorId = 1,
            Category = new Category { Name = "Main" },
            Author = new ApplicationUser { Id = 1, UserName = "chef" }
        };

        var user = new ApplicationUser { Id = 7, UserName = "tester" };

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(recipe);
        _userRepositoryMock.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(user);
        _ratingRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Rating, bool>>>(),
                It.IsAny<Expression<Func<Rating, object>>[]>() ))
            .ReturnsAsync([]);
        _ratingRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Rating>())).Returns(Task.CompletedTask);

        var result = await _service.RateRecipeAsync(20, 7, 5);

        Assert.True(result.IsSuccess);
        _ratingRepositoryMock.Verify(r => r.AddAsync(It.Is<Rating>(x => x.Score == 5 && x.RecipeId == 20 && x.UserId == 7)), Times.Once);
        _ratingRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RateRecipeAsync_ExistingRating_UpdatesRating()
    {
        var recipe = new Recipe
        {
            Id = 21,
            Title = "Cake",
            CategoryId = 1,
            AuthorId = 1,
            Category = new Category { Name = "Dessert" },
            Author = new ApplicationUser { Id = 1, UserName = "chef" }
        };

        var user = new ApplicationUser { Id = 8, UserName = "tester" };
        var existingRating = new Rating
        {
            RecipeId = 21,
            UserId = 8,
            Score = 2,
            Recipe = recipe,
            User = user
        };

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(21)).ReturnsAsync(recipe);
        _userRepositoryMock.Setup(r => r.FindByIdAsync(8)).ReturnsAsync(user);
        _ratingRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Rating, bool>>>(),
                It.IsAny<Expression<Func<Rating, object>>[]>() ))
            .ReturnsAsync([existingRating]);

        var result = await _service.RateRecipeAsync(21, 8, 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, existingRating.Score);
        _ratingRepositoryMock.Verify(r => r.Update(existingRating), Times.Once);
        _ratingRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateRecipeAsync_WrongAuthor_ReturnsFailure()
    {
        var recipe = new Recipe
        {
            Id = 7,
            AuthorId = 1,
            Title = "Old",
            Description = "Old desc",
            PrepTimeMin = 10,
            CategoryId = 1
        };

        var updateDto = new UpdateRecipeDto
        {
            Title = "New",
            Description = "New desc",
            PrepTimeMin = 25,
            Servings = 3,
            Ingredients = "Ingredient",
            CookingMethod = "Method",
            CategoryId = 2
        };

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(recipe);

        var result = await _service.UpdateRecipeAsync(7, 2, updateDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви можете редагувати тільки свої рецепти", result.ErrorMessage);
        _recipeRepositoryMock.Verify(r => r.Update(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRecipeAsync_ValidOwner_UpdatesRecipeAndReturnsSuccess()
    {
        var recipe = new Recipe
        {
            Id = 8,
            AuthorId = 4,
            Title = "Before",
            Description = "Before desc",
            PrepTimeMin = 15,
            Servings = 2,
            Ingredients = "Old ingredient",
            CookingMethod = "Old step",
            CategoryId = 1
        };

        var updateDto = new UpdateRecipeDto
        {
            Title = "After",
            Description = "After desc",
            PrepTimeMin = 30,
            Servings = 5,
            Ingredients = "New ingredient 1\nNew ingredient 2",
            CookingMethod = "Step 1\nStep 2",
            CategoryId = 3
        };

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(recipe);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(3)).ReturnsAsync(true);

        var result = await _service.UpdateRecipeAsync(8, 4, updateDto);

        Assert.True(result.IsSuccess);
        Assert.Equal("After", recipe.Title);
        Assert.Equal("After desc", recipe.Description);
        Assert.Equal(30, recipe.PrepTimeMin);
        Assert.Equal(5, recipe.Servings);
        Assert.Equal("New ingredient 1\nNew ingredient 2", recipe.Ingredients);
        Assert.Equal("Step 1\nStep 2", recipe.CookingMethod);
        Assert.Equal(3, recipe.CategoryId);
        _recipeRepositoryMock.Verify(r => r.Update(recipe), Times.Once);
        _recipeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }


    [Fact]
    public async Task GetAllRecipesAsync_RecipesExist_ReturnsMappedDtosOrderedByDateDescending()
    {
        var now = DateTime.UtcNow;
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "Older",
                AuthorId = 1,
                CreatedAt = now.AddHours(-1),
                Category = new Category { Name = "Cat1" },
                Author = new ApplicationUser { UserName = "chef1" }
            },
            new Recipe
            {
                Id = 2,
                Title = "Newer",
                AuthorId = 1,
                CreatedAt = now,
                Category = new Category { Name = "Cat2" },
                Author = new ApplicationUser { UserName = "chef2" }
            }
        };

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync(recipes);

        var result = await _service.GetAllRecipesAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Equal("Newer", result.Value!.First().Title);
    }

    [Fact]
    public async Task GetAllRecipesAsync_NoRecipes_ReturnsSuccessWithEmptyCollection()
    {
        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync([]);

        var result = await _service.GetAllRecipesAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetAllRecipesAsync_RecipesExist_MapsCategoryNameAndAuthorName()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Title = "Pasta",
            Category = new Category { Name = "Italian" },
            Author = new ApplicationUser { UserName = "chef" }
        };

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync([recipe]);

        var result = await _service.GetAllRecipesAsync();

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("Italian", dto.CategoryName);
        Assert.Equal("chef", dto.AuthorName);
    }
}
