using LaconicAndIconic.BLL.Models;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password, bool rememberMe);
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
}
