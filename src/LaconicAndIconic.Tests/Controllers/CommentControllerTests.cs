using System.Security.Claims;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class CommentControllerTests : IDisposable
{
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly CommentController _controller;

    public CommentControllerTests()
    {
        _commentServiceMock = new Mock<ICommentService>();

        _controller = new CommentController(_commentServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>())
        };

        SetUserContext(42);
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

    // --- Create tests ---

    [Fact]
    public async Task Create_ValidDto_RedirectsToRecipeDetails()
    {
        // Arrange
        var dto = new CreateCommentDto { RecipeId = 1, Content = "Great recipe!" };
        _commentServiceMock
            .Setup(s => s.CreateAsync(dto, 42))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Recipe", redirect.ControllerName);
        Assert.Equal(dto.RecipeId, redirect.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_ValidDto_CallsServiceWithCurrentUserId()
    {
        // Arrange
        var dto = new CreateCommentDto { RecipeId = 1, Content = "Great recipe!" };
        _commentServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateCommentDto>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _controller.Create(dto);

        // Assert
        _commentServiceMock.Verify(s => s.CreateAsync(dto, 42), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidModelState_RedirectsWithoutCallingService()
    {
        // Arrange
        var dto = new CreateCommentDto { RecipeId = 7, Content = string.Empty };
        _controller.ModelState.AddModelError("Content", "Required");

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Recipe", redirect.ControllerName);
        Assert.Equal(dto.RecipeId, redirect.RouteValues!["id"]);
        _commentServiceMock.Verify(s => s.CreateAsync(It.IsAny<CreateCommentDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Create_InvalidModelState_SetsTempDataErrorMessage()
    {
        // Arrange
        var dto = new CreateCommentDto { RecipeId = 7, Content = string.Empty };
        _controller.ModelState.AddModelError("Content", "Required");

        // Act
        await _controller.Create(dto);

        // Assert
        Assert.NotNull(_controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Create_ServiceReturnsFailure_SetsTempDataErrorMessage()
    {
        // Arrange
        var dto = new CreateCommentDto { RecipeId = 1, Content = "ok" };
        _commentServiceMock
            .Setup(s => s.CreateAsync(dto, 42))
            .ReturnsAsync(Result.Failure("Коментар не може бути порожнім"));

        // Act
        await _controller.Create(dto);

        // Assert
        Assert.Equal("Коментар не може бути порожнім", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Create_ServiceReturnsFailure_StillRedirectsToRecipeDetails()
    {
        // Arrange
        var dto = new CreateCommentDto { RecipeId = 3, Content = "ok" };
        _commentServiceMock
            .Setup(s => s.CreateAsync(dto, 42))
            .ReturnsAsync(Result.Failure("Error"));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Recipe", redirect.ControllerName);
        Assert.Equal(dto.RecipeId, redirect.RouteValues!["id"]);
    }
}
