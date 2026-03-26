using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace LaconicAndIconic.Tests;

public class AuthServiceTests
{
    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(Mock<UserManager<ApplicationUser>> userManagerMock)
    {
        return new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<ApplicationUser>>().Object);
    }

    private static AuthService CreateSut(
        Mock<SignInManager<ApplicationUser>> signInManagerMock,
        Mock<IUserRepository> userRepositoryMock)
        => new(signInManagerMock.Object, userRepositoryMock.Object, NullLogger<AuthService>.Instance);

    // --- RegisterAsync tests ---

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(signInManagerMock, userRepoMock);

        var request = new RegisterRequest
        {
            Email = "alice@example.com",
            UserName = "alice",
            Password = "P@ssw0rd!",
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        userRepoMock.Verify(
            r => r.CreateAsync(
                It.Is<ApplicationUser>(u => u.Email == request.Email && u.UserName == request.UserName),
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
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(duplicateError);

        var sut = CreateSut(signInManagerMock, userRepoMock);

        var request = new RegisterRequest
        {
            Email = "alice@example.com",
            UserName = "alice2",
            Password = "P@ssw0rd!",
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already taken", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var userRepoMock = new Mock<IUserRepository>();
        var sut = CreateSut(signInManagerMock, userRepoMock);

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
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(passwordError);

        var sut = CreateSut(signInManagerMock, userRepoMock);

        var request = new RegisterRequest
        {
            Email = "bob@example.com",
            UserName = "bob",
            Password = "123",
        };

        // Act
        var result = await sut.RegisterAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("8 characters", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    // --- LoginAsync tests ---

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithLoginResultSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Email = "user@example.com", UserName = "user" };

        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        signInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, "P@ssw0rd!", false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(r => r.FindByEmailAsync(user.Email)).ReturnsAsync(user);

        var sut = CreateSut(signInManagerMock, userRepoMock);

        // Act
        var result = await sut.LoginAsync(user.Email!, "P@ssw0rd!", rememberMe: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LoginResult.Success, result.Value);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsSuccessWithInvalidCredentials()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(r => r.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var sut = CreateSut(signInManagerMock, userRepoMock);

        // Act
        var result = await sut.LoginAsync("ghost@example.com", "P@ssw0rd!", rememberMe: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LoginResult.InvalidCredentials, result.Value);
    }

    [Fact]
    public async Task LoginAsync_LockedOutUser_ReturnsSuccessWithLockedOut()
    {
        // Arrange
        var user = new ApplicationUser { Email = "locked@example.com", UserName = "locked" };

        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        signInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, It.IsAny<string>(), false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(r => r.FindByEmailAsync(user.Email)).ReturnsAsync(user);

        var sut = CreateSut(signInManagerMock, userRepoMock);

        // Act
        var result = await sut.LoginAsync(user.Email!, "wrong", rememberMe: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LoginResult.LockedOut, result.Value);
    }

    // --- LogoutAsync tests ---

    [Fact]
    public async Task LogoutAsync_ReturnsSuccess()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        signInManagerMock.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        var userRepoMock = new Mock<IUserRepository>();
        var sut = CreateSut(signInManagerMock, userRepoMock);

        // Act
        var result = await sut.LogoutAsync();

        // Assert
        Assert.True(result.IsSuccess);
        signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
    }
}
