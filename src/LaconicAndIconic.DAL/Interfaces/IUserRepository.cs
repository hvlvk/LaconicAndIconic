using System.Linq.Expressions;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Models;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(int id);
    Task<UserProfileProjection?> GetUserProfileByIdAsync(int id, int? currentUserId = null);
    Task<IEnumerable<UserProfileProjection>> GetUserProfilesAsync(IEnumerable<int>? userIds = null, int? currentUserId = null);
    Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);   
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<ApplicationUser?> FindByUserNameAsync(string username);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<IEnumerable<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate);
    Task<IEnumerable<ApplicationUser>> FindAsync();
    Task<IdentityResult> DeleteAsync(ApplicationUser user);
}
