using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LaconicAndIconic.BLL.Services;

public partial class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<User> userManager, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        LogRegistrationAttempt(_logger, request.Email);

        var user = new User
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Attempting to register user with email {Email}.")]
    private static partial void LogRegistrationAttempt(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserName} registered successfully.")]
    private static partial void LogRegistrationSuccess(ILogger logger, string userName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration failed for {Email}. Errors: {Errors}.")]
    private static partial void LogRegistrationFailure(ILogger logger, string email, string errors);
}
