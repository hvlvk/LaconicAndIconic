using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(string email, string password, bool rememberMe);
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result> LogoutAsync();
    Task<Result<string>> GeneratePasswordResetTokenAsync(string email);
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword);
}
