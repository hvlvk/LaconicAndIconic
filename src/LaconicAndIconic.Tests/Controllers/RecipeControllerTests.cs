using System.Security.Claims;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class RecipeControllerTests : IDisposable
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly RecipeController _controller;

    public RecipeControllerTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        _commentServiceMock = new Mock<ICommentService>();
        var categoryServiceMock = new Mock<ICategoryService>();
        var userServiceMock = new Mock<IUserService>();

        _commentServiceMock
            .Setup(s => s.GetCommentsByRecipeIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<IEnumerable<CommentDto>>.Success([]));

        _controller = new RecipeController(
            _recipeServiceMock.Object,
            categoryServiceMock.Object,
            userServiceMock.Object,
            _commentServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        _controller.TempData = new TempDataDictionary(_controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SetUserContext(int? userId)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        var identity = new ClaimsIdentity(claims, userId.HasValue ? "TestAuth" : null);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

    }

    private static RecipeDto MakeRecipeDto(int id = 1, int authorId = 2) => new()
    {
        Id = id,
        Title = "Pasta",
        Description = "Great pasta",
        PrepTimeMin = 25,
        AverageRating = 4.5,
        RatingCount = 8,
        Servings = 4,
        Ingredients = "Pasta\nCheese",
        CookingMethod = "Boil\nServe",
        CategoryName = "Italian",
        AuthorId = authorId,
        AuthorName = "chef"
    };


    [Fact]
    public async Task Details_RecipeExists_ReturnsViewWithRecipeDetailsViewModel()
    {
        // Arrange
        var dto = MakeRecipeDto();
        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(dto.Id, It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));

        // Act
        var result = await _controller.Details(dto.Id);

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

    [Fact]
    public async Task Details_CommentsExist_ReturnsViewModelWithComments()
    {
        // Arrange
        var dto = MakeRecipeDto();
        var comments = new List<CommentDto>
        {
            new() { Id = 1, RecipeId = dto.Id, AuthorId = 5, AuthorName = "alice", Content = "Delicious!", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, RecipeId = dto.Id, AuthorId = 6, AuthorName = "bob",   Content = "Loved it.",  CreatedAt = DateTime.UtcNow }
        };

        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(dto.Id, It.IsAny<int?>()))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));
        _commentServiceMock
            .Setup(s => s.GetCommentsByRecipeIdAsync(dto.Id))
            .ReturnsAsync(Result<IEnumerable<CommentDto>>.Success(comments));

        // Act
        var result = await _controller.Details(dto.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);
        Assert.Equal(2, model.Comments.Count);
        Assert.Equal("alice", model.Comments[0].AuthorName);
        Assert.Equal("bob", model.Comments[1].AuthorName);
    }
}
