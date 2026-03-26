using LaconicAndIconic.BLL.Models;
using Microsoft.AspNetCore.Http;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IUserService
{
    Task<Result<UserProfileDto>> GetUserProfileByIdAsync(int id);
    Task<Result<string>> UpdateProfilePictureAsync(int id, IFormFile file);
}
