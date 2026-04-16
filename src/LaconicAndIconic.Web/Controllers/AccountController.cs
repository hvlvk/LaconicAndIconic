using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model.Email, model.Password, model.RememberMe);

        switch (result.Value)
        {
            case LoginResult.Success:
                return RedirectToLocal(returnUrl);

            case LoginResult.LockedOut:
                ModelState.AddModelError(string.Empty, "Account is locked out. Please try again later.");
                return View(model);

            default:
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new RegisterRequest
        {
            Email = model.Email,
            UserName = model.UserName,
            Password = model.Password,
        };

        var result = await _authService.RegisterAsync(request);

        if (result.IsSuccess)
        {
            await _authService.LoginAsync(model.Email, model.Password, rememberMe: false);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.GeneratePasswordResetTokenAsync(model.Email);

        if (result.IsSuccess)
        {
            var resetLink = Url.Action("ResetPassword", "Account",
                new { token = result.Value, email = model.Email }, Request.Scheme);
            TempData["ResetLink"] = resetLink;
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token = null, string? email = null)
    {
        if (token is null)
        {
            return BadRequest("A token must be supplied for password reset.");
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email ?? string.Empty,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ResetPasswordAsync(model.Email, model.Token, model.Password);

        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Password reset failed.");
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
