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

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<ApplicationUser, bool>> predicate)
        => Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(_userManager.Users, predicate);

    public Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        => _userManager.CreateAsync(user, password);

    public Task<IdentityResult> UpdateAsync(ApplicationUser user)
        => _userManager.UpdateAsync(user);

    public Task<ApplicationUser?> FindByUserNameAsync(string username)
        => _userManager.FindByNameAsync(username);

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        => _userManager.GeneratePasswordResetTokenAsync(user);

    public Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        => _userManager.ResetPasswordAsync(user, token, newPassword);

    public async Task<IEnumerable<ApplicationUser>> FindAsync(System.Linq.Expressions.Expression<Func<ApplicationUser, bool>> predicate)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(_userManager.Users.Where(predicate));
    }
}
