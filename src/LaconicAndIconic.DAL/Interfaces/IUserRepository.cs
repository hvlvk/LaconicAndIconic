using System.Linq.Expressions;
using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(int id);
    Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);   
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<ApplicationUser?> FindByUserNameAsync(string username);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<IEnumerable<ApplicationUser>> FindAsync(Expression<Func<ApplicationUser, bool>> predicate);
}
