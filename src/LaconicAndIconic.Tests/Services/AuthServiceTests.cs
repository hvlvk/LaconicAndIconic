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

namespace LaconicAndIconic.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthService _sut;
    public AuthServiceTests()
    {
        var userManagerMock = CreateUserManagerMock();
        _signInManagerMock = CreateSignInManagerMock(userManagerMock);
        _userRepositoryMock = new Mock<IUserRepository>();
        _sut = new AuthService(
            _signInManagerMock.Object,
            _userRepositoryMock.Object,
            NullLogger<AuthService>.Instance
        );
    }
    
    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var email = "alice@example.com";
        var userName = "alice";
        var password = "P@ssw0rd!";

        var request = new RegisterRequest
        {
            Email = email,
            UserName = userName,
            Password = password,
        };

        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepositoryMock.Verify(
            r => r.CreateAsync(
                It.Is<ApplicationUser>(u => u.Email == email && u.UserName == userName),
                password),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailureWithError()
    {
        // Arrange
        var email = "alice@example.com";
        var password = "P@ssw0rd!";
        var request = new RegisterRequest { Email = email, UserName = "alice2", Password = password };

        var duplicateError = IdentityResult.Failed(
            new IdentityError { Code = "DuplicateEmail", Description = $"Email '{email}' is already taken." });

        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(duplicateError);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already taken", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.RegisterAsync(null!));
    }

    [Fact]
    public async Task RegisterAsync_PasswordTooShort_ReturnsFailureWithError()
    {
        // Arrange
        var password = "123";
        var request = new RegisterRequest { Email = "bob@example.com", UserName = "bob", Password = password };

        var passwordError = IdentityResult.Failed(
            new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 8 characters." });

        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(passwordError);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("8 characters", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithLoginResultSuccess()
    {
        // Arrange
        var email = "user@example.com";
        var password = "P@ssw0rd!";
        var user = new ApplicationUser { Email = email, UserName = "user" };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, password, false, true))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var result = await _sut.LoginAsync(email, password, rememberMe: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LoginResult.Success, result.Value);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsSuccessWithInvalidCredentials()
    {
        // Arrange
        var email = "ghost@example.com";
        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.LoginAsync(email, "P@ssw0rd!", rememberMe: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LoginResult.InvalidCredentials, result.Value);
    }

    [Fact]
    public async Task LoginAsync_LockedOutUser_ReturnsSuccessWithLockedOut()
    {
        // Arrange
        var email = "locked@example.com";
        var password = "wrongpassword";
        var user = new ApplicationUser { Email = email, UserName = "locked" };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, password, false, true))
            .ReturnsAsync(SignInResult.LockedOut);

        // Act
        var result = await _sut.LoginAsync(email, password, rememberMe: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LoginResult.LockedOut, result.Value);
    }


    [Fact]
    public async Task LogoutAsync_ReturnsSuccess()
    {
        // Arrange
        _signInManagerMock
            .Setup(s => s.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LogoutAsync();

        // Assert
        Assert.True(result.IsSuccess);
        _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
    }


    [Fact]
    public async Task GeneratePasswordResetTokenAsync_ValidEmail_ReturnsToken()
    {
        // Arrange
        var email = "user@example.com";
        var expectedToken = "reset-token-123";
        var user = new ApplicationUser { Email = email, UserName = "user" };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _sut.GeneratePasswordResetTokenAsync(email);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value);
        _userRepositoryMock.Verify(r => r.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var email = "ghost@example.com";
        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.GeneratePasswordResetTokenAsync(email);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(r => r.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }


    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var email = "user@example.com";
        var token = "valid-token";
        var newPassword = "NewP@ss1!";
        var user = new ApplicationUser { Email = email, UserName = "user" };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ResetPasswordAsync(email, token, newPassword);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepositoryMock.Verify(r => r.ResetPasswordAsync(user, token, newPassword), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var email = "ghost@example.com";
        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.ResetPasswordAsync(email, "token", "NewP@ss1!");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var email = "user@example.com";
        var badToken = "bad-token";
        var newPassword = "NewP@ss1!";
        var user = new ApplicationUser { Email = email, UserName = "user" };

        var tokenError = IdentityResult.Failed(
            new IdentityError { Code = "InvalidToken", Description = "Invalid token." });

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.ResetPasswordAsync(user, badToken, newPassword))
            .ReturnsAsync(tokenError);

        // Act
        var result = await _sut.ResetPasswordAsync(email, badToken, newPassword);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid token", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
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
}
