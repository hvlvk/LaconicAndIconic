using LaconicAndIconic.BLL.Models;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
}
