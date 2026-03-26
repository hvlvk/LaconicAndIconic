using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
}
