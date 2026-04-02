using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Moq;
using Xunit;

namespace LaconicAndIconic.Tests.Services;

public class RecipeServiceSearchTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IRepository<Category>> _categoryRepoMock;
    private readonly RecipeService _service;

    public RecipeServiceSearchTests()
    {
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _fileServiceMock = new Mock<IFileService>();
        _categoryRepoMock = new Mock<IRepository<Category>>();
        _service = new RecipeService(_recipeRepoMock.Object, _fileServiceMock.Object, _categoryRepoMock.Object);
    }

    private static void SetupSearch(Mock<IRecipeRepository> mock, IEnumerable<Recipe> recipes, int totalCount)
    {
        mock.Setup(r => r.SearchAsync(
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((recipes, totalCount));
    }

    [Fact]
    public async Task SearchRecipesAsync_ReturnsMappedDtos()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Борщ зі сметаною", Category = new Category { Name = "Супи" }, Author = new ApplicationUser { UserName = "chef" } }
        };
        SetupSearch(_recipeRepoMock, recipes, 1);

        var filter = new RecipeSearchFilterDto { SearchTerm = "борщ", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Recipes);
        Assert.Equal("Борщ зі сметаною", result.Value.Recipes[0].Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_PassesCategoryFilterToRepository()
    {
        // Arrange
        var categoryId = 1;
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Сирники", CategoryId = categoryId, Category = new Category { Name = "Сніданок" }, Author = new ApplicationUser { UserName = "chef" } }
        };

        _recipeRepoMock.Setup(r => r.SearchAsync(
                It.IsAny<string?>(),
                categoryId,
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((recipes.AsEnumerable(), 1));

        var filter = new RecipeSearchFilterDto { CategoryId = categoryId, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Recipes);
        Assert.Equal(categoryId, result.Value.Recipes[0].CategoryId);
        _recipeRepoMock.Verify(r => r.SearchAsync(
            It.IsAny<string?>(), categoryId, It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SearchRecipesAsync_HandlesPagination()
    {
        // Arrange
        var pagedRecipes = Enumerable.Range(11, 5)
            .Select(i => new Recipe { Id = i, Title = $"Recipe {i}", Category = new Category { Name = "Cat" }, Author = new ApplicationUser { UserName = "chef" } })
            .ToList();

        _recipeRepoMock.Setup(r => r.SearchAsync(
                It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>(), 2, 10))
            .ReturnsAsync((pagedRecipes.AsEnumerable(), 15));

        var filter = new RecipeSearchFilterDto { PageNumber = 2, PageSize = 10 };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Recipes.Count);
        Assert.Equal(15, result.Value.TotalCount);
    }

    [Fact]
    public async Task SearchRecipesAsync_MapsResultMetadata()
    {
        // Arrange
        SetupSearch(_recipeRepoMock, [], 0);

        var filter = new RecipeSearchFilterDto
        {
            SearchTerm = "test",
            CategoryId = 3,
            SortBy = "title_asc",
            PageNumber = 2,
            PageSize = 5
        };

        // Act
        var result = await _service.SearchRecipesAsync(filter);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value!.SearchTerm);
        Assert.Equal(3, result.Value.CategoryId);
        Assert.Equal("title_asc", result.Value.SortBy);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(5, result.Value.PageSize);
    }
}
