using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class AccountControllerTests : IDisposable
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();

        _controller = new AccountController(_authServiceMock.Object);

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(u => u.IsLocalUrl(It.IsAny<string?>()))
            .Returns(false);

        _controller.Url = urlHelperMock.Object;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }

    // --- Login tests ---

    [Fact]
    public async Task Login_ValidCredentials_RedirectsToHome()
    {
        // Arrange
        var model = new LoginViewModel { Email = "user@example.com", Password = "Password1!" };
        _authServiceMock
            .Setup(s => s.LoginAsync(model.Email, model.Password, model.RememberMe))
            .ReturnsAsync(Result<LoginResult>.Success(LoginResult.Success));

        // Act
        var result = await _controller.Login(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsViewWithError()
    {
        // Arrange
        var model = new LoginViewModel { Email = "user@example.com", Password = "WrongPassword1!" };
        _authServiceMock
            .Setup(s => s.LoginAsync(model.Email, model.Password, model.RememberMe))
            .ReturnsAsync(Result<LoginResult>.Success(LoginResult.InvalidCredentials));

        // Act
        var result = await _controller.Login(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task Login_LockedOut_ReturnsViewWithLockedOutError()
    {
        // Arrange
        var model = new LoginViewModel { Email = "user@example.com", Password = "Password1!" };
        _authServiceMock
            .Setup(s => s.LoginAsync(model.Email, model.Password, model.RememberMe))
            .ReturnsAsync(Result<LoginResult>.Success(LoginResult.LockedOut));

        // Act
        var result = await _controller.Login(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task Login_InvalidModelState_ReturnsView()
    {
        // Arrange
        var model = new LoginViewModel { Email = string.Empty, Password = string.Empty };
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Login(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        _authServiceMock.Verify(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    // --- Register tests ---

    [Fact]
    public async Task Register_ValidRequest_RedirectsToHome()
    {
        // Arrange
        var model = new RegisterViewModel { Email = "alice@example.com", UserName = "alice", Password = "P@ssw0rd!", ConfirmPassword = "P@ssw0rd!" };
        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result.Success());
        _authServiceMock
            .Setup(s => s.LoginAsync(model.Email, model.Password, false))
            .ReturnsAsync(Result<LoginResult>.Success(LoginResult.Success));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public async Task Register_ServiceFailure_ReturnsViewWithError()
    {
        // Arrange
        var model = new RegisterViewModel { Email = "alice@example.com", UserName = "alice", Password = "P@ssw0rd!", ConfirmPassword = "P@ssw0rd!" };
        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result.Failure("Email is already taken."));

        // Act
        var result = await _controller.Register(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task Register_InvalidModelState_ReturnsView()
    {
        // Arrange
        var model = new RegisterViewModel { Email = string.Empty, UserName = string.Empty, Password = string.Empty, ConfirmPassword = string.Empty };
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Register(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        _authServiceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
    }

    // --- Logout tests ---

    [Fact]
    public async Task Logout_RedirectsToHome()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.LogoutAsync())
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Logout();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        _authServiceMock.Verify(s => s.LogoutAsync(), Times.Once);
    }
}

