using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.BLL.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;

    public AuthService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        return await _userManager.CreateAsync(user, request.Password);
    }
}
