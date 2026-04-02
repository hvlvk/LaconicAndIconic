using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class RecipeControllerTests : IDisposable
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly RecipeController _controller;

    public RecipeControllerTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        var categoryServiceMock = new Mock<ICategoryService>();
        var userServiceMock = new Mock<IUserService>();
        _controller = new RecipeController(_recipeServiceMock.Object, categoryServiceMock.Object, userServiceMock.Object);
    }

    public void Dispose()
    {
        _controller.Dispose();
    }

    [Fact]
    public async Task Details_RecipeExists_ReturnsViewWithRecipeDetailsViewModel()
    {
        // Arrange
        var dto = new RecipeDto
        {
            Id = 1,
            Title = "Pasta",
            Description = "Great pasta",
            PrepTimeMin = 25,
            CategoryName = "Italian",
            AuthorId = 2,
            AuthorName = "chef"
        };

        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(1))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);
        Assert.Equal(dto.Id, model.Id);
        Assert.Equal(dto.Title, model.Title);
        Assert.Equal(dto.AuthorName, model.AuthorName);
    }

    [Fact]
    public async Task Details_RecipeDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(404))
            .ReturnsAsync(Result<RecipeDto>.Failure("Recipe not found"));

        // Act
        var result = await _controller.Details(404);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

