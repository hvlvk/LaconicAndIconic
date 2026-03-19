using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        var loggerMock = new Mock<ILogger<AccountController>>();

        _controller = new AccountController(_authServiceMock.Object, loggerMock.Object);

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

    [Fact]
    public async Task Login_ValidCredentials_RedirectsToHome()
    {
        // Arrange
        var model = new LoginViewModel { Email = "user@example.com", Password = "Password1!" };
        _authServiceMock
            .Setup(s => s.LoginAsync(model.Email, model.Password, model.RememberMe))
            .ReturnsAsync(LoginResult.Success);

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
            .ReturnsAsync(LoginResult.InvalidCredentials);

        // Act
        var result = await _controller.Login(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
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
}
