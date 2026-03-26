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
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();

        _controller = new UserController(_userServiceMock.Object)
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
            .Setup(s => s.GetUserProfileByIdAsync(userId))
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
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(userId))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));

        // Act
        var result = await _controller.Profile(userId) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ViewName); // Null means it uses the default view name
        
        var model = Assert.IsType<UserProfileViewModel>(result.Model);
        Assert.Equal(userId, model.Id);
        Assert.Equal("testuser", model.UserName);
        Assert.True(model.IsOwnProfile);
    }

    [Fact]
    public async Task Profile_UserFound_OtherProfile_ReturnsViewWithIsOwnProfileFalse()
    {
        // Arrange
        var targetUserId = 2;
        var loggedInUserId = 1;
        SetUserContext(loggedInUserId);

        var userDto = new UserProfileDto { Id = targetUserId, UserName = "otheruser" };
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(targetUserId))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));

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
        _userServiceMock
            .Setup(s => s.GetUserProfileByIdAsync(targetUserId))
            .ReturnsAsync(Result<UserProfileDto>.Success(userDto));

        // Act
        var result = await _controller.Profile(targetUserId) as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = Assert.IsType<UserProfileViewModel>(result.Model);
        Assert.False(model.IsOwnProfile);
    }
}
