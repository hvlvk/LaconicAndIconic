using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class HomeControllerTests : IDisposable
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        _controller = new HomeController(NullLogger<HomeController>.Instance, _recipeServiceMock.Object);
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Index_RecipesExist_ReturnsViewWithRecipeDtoCollection()
    {
        // Arrange
        var recipeDtoStub = new RecipeDto { Id = 1, Title = "Pasta", AuthorName = "chef" };
        _recipeServiceMock
            .Setup(s => s.GetAllRecipesAsync())
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success([recipeDtoStub]));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal(recipeDtoStub.Title, model.First().Title);
    }

    [Fact]
    public async Task Index_NoRecipes_ReturnsViewWithEmptyCollection()
    {
        // Arrange
        _recipeServiceMock
            .Setup(s => s.GetAllRecipesAsync())
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success(Enumerable.Empty<RecipeDto>()));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Index_ServiceReturnsFailure_ReturnsViewWithEmptyCollection()
    {
        // Arrange
        _recipeServiceMock
            .Setup(s => s.GetAllRecipesAsync())
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Failure("error"));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(viewResult.Model);
        Assert.Empty(model);
    }
}
