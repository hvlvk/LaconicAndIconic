using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Identity; 
using System.Linq;                   

namespace LaconicAndIconic.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IRepository<UserSubscription>> _subscriptionRepositoryMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _fileServiceMock = new Mock<IFileService>();
        _subscriptionRepositoryMock = new Mock<IRepository<UserSubscription>>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(
            _userRepositoryMock.Object, 
            _fileServiceMock.Object, 
            _subscriptionRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserProfileByIdAsync_WhenUserExists_ReturnsSuccess()
    {
        var userId = 1;
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userRepositoryMock.Setup(repo => repo.FindByIdAsync(userId)).ReturnsAsync(user);

        var result = await _userService.GetUserProfileByIdAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.Id);
        Assert.Equal("testuser", result.Value.UserName);
    }

    [Fact]
    public async Task GetUserProfileByIdAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var userId = 1;
        _userRepositoryMock.Setup(repo => repo.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

        var result = await _userService.GetUserProfileByIdAsync(userId);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("Користувача не знайдено", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenAdminDeletesExistingUser_ReturnsSuccess()
    {
    var userId = 1;
    var user = new ApplicationUser { Id = userId, UserName = "TargetUser" };

    _userRepositoryMock.Setup(repo => repo.FindByIdAsync(userId))
        .ReturnsAsync(user);

    _userRepositoryMock.Setup(repo => repo.DeleteAsync(user))
        .ReturnsAsync(IdentityResult.Success);

    var result = await _userService.DeleteUserAsync(userId);

    Assert.True(result.IsSuccess);
    _userRepositoryMock.Verify(repo => repo.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenAdminDeletesNonExistentUser_ReturnsFailure()
    {
        
        var userId = 999;
        _userRepositoryMock.Setup(repo => repo.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

        var result = await _userService.DeleteUserAsync(userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено", result.ErrorMessage);
    }
}

