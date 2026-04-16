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
            LogLoginAttemptNonExistent(_logger, email);
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
        LogLogout(_logger);
        return Result.Success();
    }

    public async Task<Result<string>> GeneratePasswordResetTokenAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var user = await _userRepository.FindByEmailAsync(email);
        if (user is null)
        {
            LogPasswordResetNonExistent(_logger, email);
            return Result<string>.Failure("If an account with that email exists, a reset link has been generated.");
        }

        var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
        LogPasswordResetTokenGenerated(_logger, email);
        return Result<string>.Success(token);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(newPassword);

        var user = await _userRepository.FindByEmailAsync(email);
        if (user is null)
        {
            LogPasswordResetNonExistent(_logger, email);
            return Result.Failure("Invalid reset attempt.");
        }

        var identityResult = await _userRepository.ResetPasswordAsync(user, token, newPassword);

        if (identityResult.Succeeded)
        {
            LogPasswordResetSuccess(_logger, email);
            return Result.Success();
        }

        var errors = string.Join(" ", identityResult.Errors.Select(e => e.Description));
        LogPasswordResetFailure(_logger, email, errors);
        return Result.Failure(errors);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Attempting to register user with email {Email}.")]
    private static partial void LogRegistrationAttempt(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserName} registered successfully.")]
    private static partial void LogRegistrationSuccess(ILogger logger, string userName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration failed for {Email}. Errors: {Errors}.")]
    private static partial void LogRegistrationFailure(ILogger logger, string email, string errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Login attempt for non-existent email: {Email}.")]
    private static partial void LogLoginAttemptNonExistent(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {Email} logged in successfully.")]
    private static partial void LogLoginSuccess(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "User account locked out for email: {Email}.")]
    private static partial void LogAccountLockedOut(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid credentials for email: {Email}.")]
    private static partial void LogInvalidCredentials(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User logged out.")]
    private static partial void LogLogout(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Password reset requested for non-existent email: {Email}.")]
    private static partial void LogPasswordResetNonExistent(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset token generated for {Email}.")]
    private static partial void LogPasswordResetTokenGenerated(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset successful for {Email}.")]
    private static partial void LogPasswordResetSuccess(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Password reset failed for {Email}. Errors: {Errors}.")]
    private static partial void LogPasswordResetFailure(ILogger logger, string email, string errors);
}
