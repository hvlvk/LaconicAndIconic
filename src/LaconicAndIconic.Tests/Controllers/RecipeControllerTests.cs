using System.Security.Claims;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
    }

    private static RecipeDto MakeRecipeDto(int id = 1, int authorId = 2) => new()
    {
        Id = id,
        Title = "Pasta",
        Description = "Great pasta",
        PrepTimeMin = 25,
        CategoryName = "Italian",
        AuthorId = authorId,
        AuthorName = "chef"
    };

    // --- Details tests ---

    [Fact]
    public async Task Details_RecipeExists_ReturnsViewWithRecipeDetailsViewModel()
    {
        // Arrange
        var dto = MakeRecipeDto();
        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(dto.Id))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));

        // Act
        var result = await _controller.Details(dto.Id);

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
            .Setup(s => s.GetRecipeByIdAsync(dto.Id))
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

    [Fact]
    public async Task Details_CommentServiceFails_ReturnsViewModelWithEmptyComments()
    {
        // Arrange
        var dto = MakeRecipeDto();
        _recipeServiceMock
            .Setup(s => s.GetRecipeByIdAsync(dto.Id))
            .ReturnsAsync(Result<RecipeDto>.Success(dto));
        _commentServiceMock
            .Setup(s => s.GetCommentsByRecipeIdAsync(dto.Id))
            .ReturnsAsync(Result<IEnumerable<CommentDto>>.Failure("DB error"));

        // Act
        var result = await _controller.Details(dto.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);
        Assert.Empty(model.Comments);
    }
}
