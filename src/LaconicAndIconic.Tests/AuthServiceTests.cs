using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
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

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSucceeded()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var sut = new AuthService(signInManagerMock.Object, userManagerMock.Object, NullLogger<AuthService>.Instance);

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
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(duplicateError);

        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var sut = new AuthService(signInManagerMock.Object, userManagerMock.Object, NullLogger<AuthService>.Instance);

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
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var sut = new AuthService(signInManagerMock.Object, userManagerMock.Object, NullLogger<AuthService>.Instance);

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
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(passwordError);

        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var sut = new AuthService(signInManagerMock.Object, userManagerMock.Object, NullLogger<AuthService>.Instance);

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
