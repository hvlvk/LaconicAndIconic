using LaconicAndIconic.BLL;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace LaconicAndIconic.Tests.Services;

public class RecipeServiceSearchTests : IDisposable
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly RecipeService _service;
    private readonly MemoryCache _memoryCache;

    public RecipeServiceSearchTests()
    {
        _recipeRepoMock = new Mock<IRecipeRepository>();
        var fileServiceMock = new Mock<IFileService>();
        var categoryRepoMock = new Mock<IRepository<Category>>();
        var ratingRepoMock = new Mock<IRepository<Rating>>();
        var userRepoMock = new Mock<IUserRepository>();
        var cacheInvalidationServiceMock = new Mock<ICacheInvalidationService>();
        var cachingOptions = Options.Create(new CachingOptions());

        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _service = new RecipeService(
            _recipeRepoMock.Object,
            fileServiceMock.Object,
            categoryRepoMock.Object,
            ratingRepoMock.Object,
            userRepoMock.Object,
            _memoryCache,
            cacheInvalidationServiceMock.Object,
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
    public async Task SearchRecipesAsync_HandlesCyrillicCaseInsensitivity()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Борщ зі сметаною", Category = new Category { Name = "Супи" }, Author = new ApplicationUser { UserName = "chef" } },
            new Recipe { Id = 2, Title = "Сирники смачні", Category = new Category { Name = "Сніданки" }, Author = new ApplicationUser { UserName = "chef" } }
        };

        var searchResult = new RecipeSearchResult
        {
            Recipes = [recipes[0]],
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _recipeRepoMock.Setup(r => r.SearchAsync(It.IsAny<RecipeSearchFilter>())).ReturnsAsync(searchResult);

        var filter = new RecipeSearchFilterDto { SearchTerm = "борщ", PageNumber = 1, PageSize = 10 };

        var result = await _service.SearchRecipesAsync(filter);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Recipes);
        Assert.Equal("Борщ зі сметаною", result.Value.Recipes[0].Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_SearchesInCategoryName()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Омлет", Category = new Category { Name = "Сніданок" }, Author = new ApplicationUser { UserName = "chef" } },
            new Recipe { Id = 2, Title = "Котлета", Category = new Category { Name = "Обід" }, Author = new ApplicationUser { UserName = "chef" } }
        };

        var searchResult = new RecipeSearchResult
        {
            Recipes = [recipes[0]],
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _recipeRepoMock.Setup(r => r.SearchAsync(It.IsAny<RecipeSearchFilter>())).ReturnsAsync(searchResult);

        var filter = new RecipeSearchFilterDto { SearchTerm = "сніданок", PageNumber = 1, PageSize = 10 };

        var result = await _service.SearchRecipesAsync(filter);

        Assert.True(result.IsSuccess);
        var found = Assert.Single(result.Value!.Recipes);
        Assert.Equal("Омлет", found.Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_RespectsCategoryFilter()
    {
        var categoryId = 1;
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Сирники", CategoryId = categoryId, Category = new Category { Name = "Сніданок" }, Author = new ApplicationUser { UserName = "chef" } },
            new Recipe { Id = 2, Title = "Сирники", CategoryId = 2, Category = new Category { Name = "Десерти" }, Author = new ApplicationUser { UserName = "chef" } }
        };

        var searchResult = new RecipeSearchResult
        {
            Recipes = [recipes[0]],
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _recipeRepoMock.Setup(r => r.SearchAsync(It.IsAny<RecipeSearchFilter>())).ReturnsAsync(searchResult);

        var filter = new RecipeSearchFilterDto { SearchTerm = "сирники", CategoryId = categoryId, PageNumber = 1, PageSize = 10 };

        var result = await _service.SearchRecipesAsync(filter);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Recipes);
        Assert.Equal(categoryId, result.Value.Recipes[0].CategoryId);
    }

    [Fact]
    public async Task SearchRecipesAsync_HandlesPagination()
    {
        var recipes = new List<Recipe>();
        for (int i = 1; i <= 15; i++)
        {
            recipes.Add(new Recipe { Id = i, Title = $"Recipe {i}", Category = new Category { Name = "Cat" }, Author = new ApplicationUser { UserName = "chef" } });
        }

        var pagedRecipes = recipes.Skip(10).Take(10).ToList();
        var searchResult = new RecipeSearchResult
        {
            Recipes = pagedRecipes,
            TotalCount = 15,
            PageNumber = 2,
            PageSize = 10
        };

        _recipeRepoMock.Setup(r => r.SearchAsync(It.IsAny<RecipeSearchFilter>())).ReturnsAsync(searchResult);

        var filter = new RecipeSearchFilterDto { PageNumber = 2, PageSize = 10 };

        var result = await _service.SearchRecipesAsync(filter);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Recipes.Count);
        Assert.Equal(15, result.Value.TotalCount);
    }
}
