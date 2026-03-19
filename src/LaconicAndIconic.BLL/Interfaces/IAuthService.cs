using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password, bool rememberMe);
}
