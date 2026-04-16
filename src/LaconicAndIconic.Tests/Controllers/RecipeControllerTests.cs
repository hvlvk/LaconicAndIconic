using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

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
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.TempData = new TempDataDictionary(_controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
    }

    public void Dispose()
    {
        _controller.Dispose();
    }

    private void SetUserContext(int userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
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
            AverageRating = 4.5,
            RatingCount = 8,
            Servings = 4,
            Ingredients = "Pasta\nCheese",
            CookingMethod = "Boil\nServe",
            CategoryName = "Italian",
            AuthorId = 2,
            AuthorName = "chef"
        };

        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(1, It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);
        Assert.Equal(dto.Id, model.Id);
        Assert.Equal(dto.Title, model.Title);
        Assert.Equal(dto.AuthorName, model.AuthorName);
        Assert.Equal(dto.AverageRating, model.AverageRating);
        Assert.Equal(dto.RatingCount, model.RatingCount);
        Assert.Equal(dto.Servings, model.Servings);
        Assert.Equal(dto.Ingredients, model.Ingredients);
        Assert.Equal(dto.CookingMethod, model.CookingMethod);
    }

    [Fact]
    public async Task Details_RecipeDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(404, It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeDto>.Failure("Recipe not found"));

        // Act
        var result = await _controller.Details(404);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_NotOwner_ReturnsForbid()
    {
        // Arrange
        SetUserContext(5);

        var dto = new RecipeDto
        {
            Id = 10,
            AuthorId = 1,
            Title = "Recipe",
            Description = "Desc",
            PrepTimeMin = 20,
            Servings = 3,
            Ingredients = "Ing",
            CookingMethod = "Step",
            CategoryId = 2
        };

        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(10, It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));

        // Act
        var result = await _controller.Edit(10);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidOwner_RedirectsToDetails()
    {
        // Arrange
        SetUserContext(3);

        var dto = new RecipeDto
        {
            Id = 11,
            AuthorId = 3,
            Title = "Recipe",
            Description = "Desc",
            PrepTimeMin = 20,
            Servings = 3,
            Ingredients = "Ing",
            CookingMethod = "Step",
            CategoryId = 2
        };

        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(11, It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));

        _recipeServiceMock
            .Setup(s => s.UpdateRecipeAsync(11, 3, It.IsAny<UpdateRecipeDto>()))
            .ReturnsAsync(Result.Success());

        var model = new EditRecipeViewModel
        {
            Id = 11,
            Title = "Updated",
            Description = "Updated desc",
            PrepTimeMin = 15,
            Servings = 4,
            Ingredients = "New ing",
            CookingMethod = "New step",
            CategoryId = 2
        };

        // Act
        var result = await _controller.Edit(11, model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(11, redirect.RouteValues!["id"]);
    }

    [Fact]
    public async Task Rate_Post_ValidScore_RedirectsToDetails()
    {
        // Arrange
        SetUserContext(3);

        _recipeServiceMock
            .Setup(s => s.RateRecipeAsync(11, 3, 5))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Rate(11, 5);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(11, redirect.RouteValues!["id"]);
    }
}

