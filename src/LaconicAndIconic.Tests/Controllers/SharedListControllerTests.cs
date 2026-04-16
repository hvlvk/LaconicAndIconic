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

public sealed class SharedListControllerTests : IDisposable
{
    private readonly Mock<ISharedListService> _sharedListServiceMock;
    private readonly SharedListController _controller;

    public SharedListControllerTests()
    {
        _sharedListServiceMock = new Mock<ISharedListService>();

        _controller = new SharedListController(_sharedListServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        _controller.TempData = new TempDataDictionary(
            _controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
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
            claims.Add(new Claim(ClaimTypes.NameIdentifier,
                userId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        var identity = new ClaimsIdentity(claims, userId.HasValue ? "TestAuth" : null);
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        _controller.ControllerContext.HttpContext = httpContext;
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithLists()
    {
        // Arrange
        SetUserContext(1);
        var dtos = new List<SharedListDto>
        {
            new() { Id = 1, Title = "Weekend Meals", OwnerName = "chef", MemberCount = 2, RecipeCount = 5 },
            new() { Id = 2, Title = "Party Snacks", OwnerName = "bob", MemberCount = 1, RecipeCount = 3 }
        };
        _sharedListServiceMock
            .Setup(s => s.GetListsByUserAsync(1))
            .ReturnsAsync(Result<IEnumerable<SharedListDto>>.Success(dtos));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<SharedListViewModel>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Index_ServiceFailure_ReturnsEmptyView()
    {
        // Arrange
        SetUserContext(1);
        _sharedListServiceMock
            .Setup(s => s.GetListsByUserAsync(1))
            .ReturnsAsync(Result<IEnumerable<SharedListDto>>.Failure("Error"));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<SharedListViewModel>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Details_Found_ReturnsViewWithDetail()
    {
        // Arrange
        SetUserContext(1);
        var detail = new SharedListDetailDto
        {
            Id = 1,
            Title = "Italian Recipes",
            Description = "Best Italian dishes",
            OwnerId = 1,
            OwnerName = "chef",
            Members = new List<SharedListMemberDto>().AsReadOnly(),
            Recipes = new List<SharedListRecipeItemDto>().AsReadOnly()
        };
        _sharedListServiceMock
            .Setup(s => s.GetByIdAsync(1, 1))
            .ReturnsAsync(Result<SharedListDetailDto>.Success(detail));

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SharedListDetailViewModel>(viewResult.Model);
        Assert.Equal("Italian Recipes", model.Title);
        Assert.True(model.IsOwner);
    }

    [Fact]
    public async Task Details_NotFound_ReturnsNotFound()
    {
        // Arrange
        SetUserContext(1);
        _sharedListServiceMock
            .Setup(s => s.GetByIdAsync(999, 1))
            .ReturnsAsync(Result<SharedListDetailDto>.Failure("Список не знайдено"));

        // Act
        var result = await _controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<CreateSharedListViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToDetails()
    {
        // Arrange
        SetUserContext(1);
        var model = new CreateSharedListViewModel { Title = "New List", Description = "Description" };
        var dto = new SharedListDto { Id = 42, Title = "New List" };
        _sharedListServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateSharedListDto>(), 1))
            .ReturnsAsync(Result<SharedListDto>.Success(dto));

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(42, redirect.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        SetUserContext(1);
        var model = new CreateSharedListViewModel { Title = "" };
        _controller.ModelState.AddModelError("Title", "Required");

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<CreateSharedListViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Edit_Get_OwnerAccess_ReturnsView()
    {
        // Arrange
        SetUserContext(1);
        var detail = new SharedListDetailDto
        {
            Id = 1,
            Title = "My List",
            Description = "Desc",
            OwnerId = 1,
            OwnerName = "chef",
            Members = new List<SharedListMemberDto>().AsReadOnly(),
            Recipes = new List<SharedListRecipeItemDto>().AsReadOnly()
        };
        _sharedListServiceMock
            .Setup(s => s.GetByIdAsync(1, 1))
            .ReturnsAsync(Result<SharedListDetailDto>.Success(detail));

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditSharedListViewModel>(viewResult.Model);
        Assert.Equal("My List", model.Title);
    }

    [Fact]
    public async Task Edit_Get_NonOwner_ReturnsForbid()
    {
        // Arrange
        SetUserContext(99);
        var detail = new SharedListDetailDto
        {
            Id = 1,
            Title = "Not Mine",
            OwnerId = 1,
            OwnerName = "chef",
            Members = new List<SharedListMemberDto>().AsReadOnly(),
            Recipes = new List<SharedListRecipeItemDto>().AsReadOnly()
        };
        _sharedListServiceMock
            .Setup(s => s.GetByIdAsync(1, 99))
            .ReturnsAsync(Result<SharedListDetailDto>.Success(detail));

        // Act
        var result = await _controller.Edit(1);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_Success_RedirectsToIndex()
    {
        // Arrange
        SetUserContext(1);
        _sharedListServiceMock
            .Setup(s => s.DeleteAsync(1, 1))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Список видалено", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task InviteUser_Success_RedirectsToDetails()
    {
        // Arrange
        SetUserContext(1);
        _sharedListServiceMock
            .Setup(s => s.InviteUserAsync(1, 1, "bob"))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.InviteUser(1, "bob");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(1, redirect.RouteValues!["id"]);
        Assert.Equal("Користувача запрошено", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task AddRecipe_Success_RedirectsToDetails()
    {
        // Arrange
        SetUserContext(1);
        _sharedListServiceMock
            .Setup(s => s.AddRecipeAsync(5, 1, 10))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.AddRecipe(5, 10);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(5, redirect.RouteValues!["id"]);
        Assert.Equal("Рецепт додано до списку", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task RemoveRecipe_Failure_SetsErrorTempData()
    {
        // Arrange
        SetUserContext(1);
        _sharedListServiceMock
            .Setup(s => s.RemoveRecipeAsync(5, 1, 10))
            .ReturnsAsync(Result.Failure("Рецепт не знайдено у списку"));

        // Act
        var result = await _controller.RemoveRecipe(5, 10);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Рецепт не знайдено у списку", _controller.TempData["ErrorMessage"]);
    }
}
