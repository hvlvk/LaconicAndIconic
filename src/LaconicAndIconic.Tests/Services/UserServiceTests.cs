using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace LaconicAndIconic.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _fileServiceMock = new Mock<IFileService>();
        _userService = new UserService(_userRepositoryMock.Object, _fileServiceMock.Object);
    }

    [Fact]
    public async Task GetUserProfileByIdAsync_WhenUserExists_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        _userRepositoryMock.Setup(repo => repo.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserProfileByIdAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.Id);
        Assert.Equal("testuser", result.Value.UserName);
    }

    [Fact]
    public async Task GetUserProfileByIdAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        _userRepositoryMock.Setup(repo => repo.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _userService.GetUserProfileByIdAsync(userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("User not found", result.ErrorMessage);
    }
}
