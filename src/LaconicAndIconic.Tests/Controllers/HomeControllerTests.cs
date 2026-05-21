using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class HomeControllerTests : IDisposable
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _controller = new HomeController(NullLogger<HomeController>.Instance, _recipeServiceMock.Object, _categoryServiceMock.Object);
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Index_RecipesExist_ReturnsViewWithRecipeListViewModel()
    {
        // Arrange
        var recipeDtoStub = new RecipeDto { Id = 1, Title = "Pasta", AuthorName = "chef" };
        var searchResult = new RecipeSearchResultDto
        {
            Recipes = [recipeDtoStub],
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _recipeServiceMock
            .Setup(s => s.SearchRecipesAsync(It.IsAny<RecipeSearchFilterDto>(), It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeSearchResultDto>.Success(searchResult));

        _categoryServiceMock
            .Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(Result<IEnumerable<CategoryDto>>.Success([]));

        // Act
        var result = await _controller.Index(null, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<RecipeListViewModel>(viewResult.Model);
        Assert.Single(model.Recipes);
        Assert.Equal(recipeDtoStub.Title, model.Recipes[0].Title);
    }

    [Fact]
    public async Task Index_NoRecipes_ReturnsViewWithEmptyRecipeCollection()
    {
        // Arrange
        var searchResult = new RecipeSearchResultDto { Recipes = [], TotalCount = 0 };
        _recipeServiceMock
            .Setup(s => s.SearchRecipesAsync(It.IsAny<RecipeSearchFilterDto>(), It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeSearchResultDto>.Success(searchResult));

        _categoryServiceMock
            .Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(Result<IEnumerable<CategoryDto>>.Success([]));

        // Act
        var result = await _controller.Index(null, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<RecipeListViewModel>(viewResult.Model);
        Assert.Empty(model.Recipes);
    }

    [Fact]
    public async Task Index_ServiceReturnsFailure_ReturnsViewWithEmptyRecipeCollection()
    {
        // Arrange
        _recipeServiceMock
            .Setup(s => s.SearchRecipesAsync(It.IsAny<RecipeSearchFilterDto>(), It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeSearchResultDto>.Failure("error"));

        _categoryServiceMock
            .Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(Result<IEnumerable<CategoryDto>>.Success([]));

        // Act
        var result = await _controller.Index(null, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<RecipeListViewModel>(viewResult.Model);
        Assert.Empty(model.Recipes);
    }
}
