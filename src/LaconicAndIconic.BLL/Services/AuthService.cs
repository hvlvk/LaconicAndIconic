using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LaconicAndIconic.BLL.Services;

public partial class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AuthService> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, bool rememberMe)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Login attempt for non-existent email: {Email}", email);
            return LoginResult.InvalidCredentials;
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully", email);
            return LoginResult.Success;
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out for email: {Email}", email);
            return LoginResult.LockedOut;
        }

        _logger.LogWarning("Invalid credentials for email: {Email}", email);
        return LoginResult.InvalidCredentials;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LogRegistrationAttempt(_logger, request.Email);

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        var result = await _userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

        if (result.Succeeded)
        {
            LogRegistrationSuccess(_logger, request.UserName);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            LogRegistrationFailure(_logger, request.Email, errors);
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Attempting to register user with email {Email}.")]
    private static partial void LogRegistrationAttempt(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserName} registered successfully.")]
    private static partial void LogRegistrationSuccess(ILogger logger, string userName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration failed for {Email}. Errors: {Errors}.")]
    private static partial void LogRegistrationFailure(ILogger logger, string email, string errors);
}
