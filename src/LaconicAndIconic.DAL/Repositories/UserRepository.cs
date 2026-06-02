using System.Globalization;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using LaconicAndIconic.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<UserProfileProjection?> GetUserProfileByIdAsync(int id, int? currentUserId = null)
    {
        return await _userManager.Users
            .Where(u => u.Id == id)
            .Select(u => new UserProfileProjection
            {
                User = u,
                FollowerCount = u.Followers.Count,
                FollowingCount = u.Following.Count,
                IsSubscribed = currentUserId.HasValue && u.Followers.Any(f => f.FollowerId == currentUserId.Value)
            }).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserProfileProjection>> GetUserProfilesAsync(IEnumerable<int>? userIds = null, int? currentUserId = null)
    {
        var query = _userManager.Users.AsQueryable();
        if (userIds != null)
        {
            query = query.Where(u => userIds.Contains(u.Id));
        }

        return await query.Select(u => new UserProfileProjection
        {
            User = u,
            FollowerCount = u.Followers.Count,
            FollowingCount = u.Following.Count,
            IsSubscribed = currentUserId.HasValue && u.Followers.Any(f => f.FollowerId == currentUserId.Value)
        }).ToListAsync();
    }

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<ApplicationUser, bool>> predicate)
        => EntityFrameworkQueryableExtensions.AnyAsync(_userManager.Users, predicate);

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
        return await EntityFrameworkQueryableExtensions.ToListAsync(_userManager.Users.Where(predicate));
    }

    public async Task<IEnumerable<ApplicationUser>> FindAsync()
    {
        return await EntityFrameworkQueryableExtensions.ToListAsync(_userManager.Users);
    }

    public Task<IdentityResult> DeleteAsync(ApplicationUser user)
        => _userManager.DeleteAsync(user);
}
