using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using MockQueryable.Moq;
using MockQueryable.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LaconicAndIconic.Tests.Services;

public class RecipeServiceSearchTests
{
    private readonly Mock<IRepository<Recipe>> _recipeRepoMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IRepository<Category>> _categoryRepoMock;
    private readonly RecipeService _service;

    public RecipeServiceSearchTests()
    {
        _recipeRepoMock = new Mock<IRepository<Recipe>>();
        _fileServiceMock = new Mock<IFileService>();
        _categoryRepoMock = new Mock<IRepository<Category>>();
        _service = new RecipeService(_recipeRepoMock.Object, _fileServiceMock.Object, _categoryRepoMock.Object);
    }

    [Fact]
    public async Task SearchRecipesAsync_HandlesCyrillicCaseInsensitivity()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Борщ зі сметаною", Category = new Category { Name = "Супи" }, Author = new ApplicationUser { UserName = "chef" } },
            new Recipe { Id = 2, Title = "Сирники смачні", Category = new Category { Name = "Сніданки" }, Author = new ApplicationUser { UserName = "chef" } }
        };

        var mock = recipes.BuildMock();
        _recipeRepoMock.Setup(r => r.GetQueryable()).Returns(mock);

        var filter = new RecipeSearchFilterDto { SearchTerm = "борщ", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Recipes);
        Assert.Equal("Борщ зі сметаною", result.Value.Recipes[0].Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_SearchesInCategoryName()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Омлет", Category = new Category { Name = "Сніданок" }, Author = new ApplicationUser { UserName = "chef" } },
            new Recipe { Id = 2, Title = "Котлета", Category = new Category { Name = "Обід" }, Author = new ApplicationUser { UserName = "chef" } }
        };

        var mock = recipes.BuildMock();
        _recipeRepoMock.Setup(r => r.GetQueryable()).Returns(mock);

        var filter = new RecipeSearchFilterDto { SearchTerm = "сніданок", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        var found = Assert.Single(result.Value!.Recipes);
        Assert.Equal("Омлет", found.Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_RespectsCategoryFilter()
    {
        // Arrange
        var categoryId = 1;
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Сирники", CategoryId = categoryId, Category = new Category { Name = "Сніданок" }, Author = new ApplicationUser { UserName = "chef" } },
            new Recipe { Id = 2, Title = "Сирники", CategoryId = 2, Category = new Category { Name = "Десерти" }, Author = new ApplicationUser { UserName = "chef" } }
        };

        var mock = recipes.BuildMock();
        _recipeRepoMock.Setup(r => r.GetQueryable()).Returns(mock);

        var filter = new RecipeSearchFilterDto { SearchTerm = "сирники", CategoryId = categoryId, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Recipes);
        Assert.Equal(categoryId, result.Value.Recipes[0].CategoryId);
    }

    [Fact]
    public async Task SearchRecipesAsync_HandlesPagination()
    {
        // Arrange
        var recipes = new List<Recipe>();
        for (int i = 1; i <= 15; i++)
        {
            recipes.Add(new Recipe { Id = i, Title = $"Recipe {i}", Category = new Category { Name = "Cat" }, Author = new ApplicationUser { UserName = "chef" } });
        }

        var mock = recipes.AsQueryable().BuildMock();
        _recipeRepoMock.Setup(r => r.GetQueryable()).Returns(mock);

        var filter = new RecipeSearchFilterDto { PageNumber = 2, PageSize = 10 };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Recipes.Count);
        Assert.Equal(15, result.Value.TotalCount);
    }
}
