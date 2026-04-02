using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LaconicAndIconic.BLL.Services;

public partial class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        SignInManager<ApplicationUser> signInManager,
        IUserRepository userRepository,
        ILogger<AuthService> logger)
    {
        _signInManager = signInManager;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<LoginResult>> LoginAsync(string email, string password, bool rememberMe)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user is null)
        {
            LogLoginAttemptForNonexistentEmail(_logger, email);
            return Result<LoginResult>.Success(LoginResult.InvalidCredentials);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            LogLoginSuccess(_logger, email);
            return Result<LoginResult>.Success(LoginResult.Success);
        }

        if (signInResult.IsLockedOut)
        {
            LogAccountLockedOut(_logger, email);
            return Result<LoginResult>.Success(LoginResult.LockedOut);
        }

        LogInvalidCredentials(_logger, email);
        return Result<LoginResult>.Success(LoginResult.InvalidCredentials);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LogRegistrationAttempt(_logger, request.Email);

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        var identityResult = await _userRepository.CreateAsync(user, request.Password);

        if (identityResult.Succeeded)
        {
            LogRegistrationSuccess(_logger, request.UserName);
            return Result.Success();
        }

        var errors = string.Join(" ", identityResult.Errors.Select(e => e.Description));
        LogRegistrationFailure(_logger, request.Email, errors);
        return Result.Failure(errors);
    }

    public async Task<Result> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        LogLogoutSuccess(_logger);
        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Login attempt for non-existent email: {Email}.")]
    private static partial void LogLoginAttemptForNonexistentEmail(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {Email} logged in successfully.")]
    private static partial void LogLoginSuccess(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "User account locked out for email: {Email}.")]
    private static partial void LogAccountLockedOut(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid credentials for email: {Email}.")]
    private static partial void LogInvalidCredentials(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User logged out.")]
    private static partial void LogLogoutSuccess(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Attempting to register user with email {Email}.")]
    private static partial void LogRegistrationAttempt(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserName} registered successfully.")]
    private static partial void LogRegistrationSuccess(ILogger logger, string userName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration failed for {Email}. Errors: {Errors}.")]
    private static partial void LogRegistrationFailure(ILogger logger, string email, string errors);
}
