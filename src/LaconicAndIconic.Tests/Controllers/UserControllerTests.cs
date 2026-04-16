using System.Security.Claims;
using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class UserControllerTests : IDisposable
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly Mock<ISharedListService> _sharedListServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _recipeServiceMock = new Mock<IRecipeService>();
        _sharedListServiceMock = new Mock<ISharedListService>();

        _controller = new UserController(
            _userServiceMock.Object,
            _recipeServiceMock.Object,
            _sharedListServiceMock.Object)
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
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext.HttpContext.User = claimsPrincipal;
    }

    [Fact]
    public async Task Profile_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(userId, It.IsAny<int?>()))
            .ReturnsAsync(Result<UserProfileDto>.Failure("Not found"));

        // Act
        var result = await _controller.Profile(userId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Profile_UserFound_OwnProfile_ReturnsViewWithIsOwnProfileTrue()
    {
        // Arrange
        var userId = 1;
        SetUserContext(userId);

        var userDto = new UserProfileDto { Id = userId, UserName = "testuser" };
        var recipesDto = new List<RecipeDto>();
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(userId, It.IsAny<int?>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));
            
        _recipeServiceMock
            .Setup(s => s.GetRecipesByAuthorIdAsync(userId))
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success(recipesDto));
        
        _sharedListServiceMock
            .Setup(s => s.GetListsByUserAsync(userId))
            .ReturnsAsync(Result<IEnumerable<SharedListDto>>.Success([]));

        // Act
        var result = await _controller.Profile(userId) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ViewName); // Null means it uses the default view name
        
        var model = Assert.IsType<UserProfileViewModel>(result.Model);
        Assert.Equal(userId, model.Id);
        Assert.Equal("testuser", model.UserName);
        Assert.True(model.IsOwnProfile);
        Assert.Equal("recipes", model.ActiveTab);
    }

    [Fact]
    public async Task Profile_UserFound_OtherProfile_ReturnsViewWithIsOwnProfileFalse()
    {
        // Arrange
        var targetUserId = 2;
        var loggedInUserId = 1;
        SetUserContext(loggedInUserId);

        var userDto = new UserProfileDto { Id = targetUserId, UserName = "otheruser" };
        var recipesDto = new List<RecipeDto>();
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(targetUserId, It.IsAny<int?>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));
            
        _recipeServiceMock
            .Setup(s => s.GetRecipesByAuthorIdAsync(targetUserId))
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success(recipesDto));

        // Act
        var result = await _controller.Profile(targetUserId) as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<UserProfileViewModel>(result.Model);
        Assert.Equal(targetUserId, model.Id);
        Assert.Equal("otheruser", model.UserName);
        Assert.False(model.IsOwnProfile);
    }

    [Fact]
    public async Task Profile_UserFound_Unauthenticated_ReturnsViewWithIsOwnProfileFalse()
    {
        // Arrange
        var targetUserId = 2;
        SetUserContext(null); // Unauthenticated

        var userDto = new UserProfileDto { Id = targetUserId, UserName = "otheruser" };
        var recipesDto = new List<RecipeDto>();
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(targetUserId, It.IsAny<int?>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));
            
        _recipeServiceMock
            .Setup(s => s.GetRecipesByAuthorIdAsync(targetUserId))
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success(recipesDto));

        // Act
        var result = await _controller.Profile(targetUserId) as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<UserProfileViewModel>(result.Model);
        Assert.False(model.IsOwnProfile);
    }

    [Fact]
    public async Task Profile_OwnProfileListsTab_LoadsSharedListsAndSelectedList()
    {
        // Arrange
        var userId = 1;
        SetUserContext(userId);

        var userDto = new UserProfileDto { Id = userId, UserName = "testuser" };
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(userId, It.IsAny<int?>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));

        _recipeServiceMock
            .Setup(s => s.GetRecipesByAuthorIdAsync(userId))
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success([]));

        _sharedListServiceMock
            .Setup(s => s.GetListsByUserAsync(userId))
            .ReturnsAsync(Result<IEnumerable<SharedListDto>>.Success([
                new SharedListDto
                {
                    Id = 10,
                    Title = "Вечеря",
                    OwnerId = userId,
                    OwnerName = "testuser",
                    MemberCount = 1,
                    RecipeCount = 2
                }
            ]));

        _sharedListServiceMock
            .Setup(s => s.GetByIdAsync(10, userId))
            .ReturnsAsync(Result<SharedListDetailDto>.Success(new SharedListDetailDto
            {
                Id = 10,
                Title = "Вечеря",
                OwnerId = userId,
                OwnerName = "testuser",
                Members = [],
                Recipes = []
            }));

        // Act
        var result = await _controller.Profile(userId, "lists", 10) as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<UserProfileViewModel>(result.Model);
        Assert.Equal("lists", model.ActiveTab);
        Assert.Single(model.SharedLists);
        Assert.NotNull(model.SelectedSharedList);
        Assert.Equal(10, model.SelectedSharedList.Id);
        _sharedListServiceMock.Verify(s => s.GetByIdAsync(10, userId), Times.Once);
    }

    [Fact]
    public async Task Profile_OtherProfileListsTab_FallsBackToRecipesTab()
    {
        // Arrange
        SetUserContext(1);
        var targetUserId = 2;
        var userDto = new UserProfileDto { Id = targetUserId, UserName = "otheruser" };

        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(targetUserId, It.IsAny<int?>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));

        _recipeServiceMock
            .Setup(s => s.GetRecipesByAuthorIdAsync(targetUserId))
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success([]));

        // Act
        var result = await _controller.Profile(targetUserId, "lists") as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<UserProfileViewModel>(result.Model);
        Assert.Equal("recipes", model.ActiveTab);
        Assert.False(model.IsOwnProfile);
        _sharedListServiceMock.Verify(s => s.GetListsByUserAsync(It.IsAny<int>()), Times.Never);
    }
}
