using System.Globalization;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<ApplicationUser?> FindByEmailAsync(string email)
        => _userManager.FindByEmailAsync(email);

    public Task<ApplicationUser?> FindByIdAsync(int id)
        => _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));

    public Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        => _userManager.CreateAsync(user, password);

    public Task<IdentityResult> UpdateAsync(ApplicationUser user)
        => _userManager.UpdateAsync(user);

    public Task<ApplicationUser?> FindByUserNameAsync(string username)
        => _userManager.FindByNameAsync(username);
}
