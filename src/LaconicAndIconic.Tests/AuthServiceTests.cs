using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LaconicAndIconic.Tests;

public class AuthServiceTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSucceeded()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new AuthService(userManagerMock.Object, NullLogger<AuthService>.Instance);

        var request = new RegisterRequest
        {
            Email = "alice@example.com",
            UserName = "alice",
            Password = "P@ssw0rd!",
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        userManagerMock.Verify(
            m => m.CreateAsync(
                It.Is<User>(u => u.Email == request.Email && u.UserName == request.UserName),
                request.Password),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailureWithError()
    {
        // Arrange
        var duplicateError = IdentityResult.Failed(
            new IdentityError { Code = "DuplicateEmail", Description = "Email 'alice@example.com' is already taken." });

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(duplicateError);

        var sut = new AuthService(userManagerMock.Object, NullLogger<AuthService>.Instance);

        var request = new RegisterRequest
        {
            Email = "alice@example.com",
            UserName = "alice2",
            Password = "P@ssw0rd!",
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "DuplicateEmail");
    }

    [Fact]
    public async Task RegisterAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var sut = new AuthService(userManagerMock.Object, NullLogger<AuthService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.RegisterAsync(null!));
    }

    [Fact]
    public async Task RegisterAsync_PasswordTooShort_ReturnsFailureWithError()
    {
        // Arrange
        var passwordError = IdentityResult.Failed(
            new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 8 characters." });

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(passwordError);

        var sut = new AuthService(userManagerMock.Object, NullLogger<AuthService>.Instance);

        var request = new RegisterRequest
        {
            Email = "bob@example.com",
            UserName = "bob",
            Password = "123",
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "PasswordTooShort");
    }
}
