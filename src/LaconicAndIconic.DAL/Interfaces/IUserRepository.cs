using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(int id);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
}
