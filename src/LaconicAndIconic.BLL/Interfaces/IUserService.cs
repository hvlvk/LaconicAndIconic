using LaconicAndIconic.BLL.Models;
using Microsoft.AspNetCore.Http;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IUserService
{
    Task<Result<UserProfileDto>> GetUserProfileByIdAsync(int id, int? currentUserId = null);
    Task<Result<string>> UpdateProfilePictureAsync(int id, IFormFile file);
    Task<Result> SubscribeAsync(int followerId, int userId);
    Task<Result> UnsubscribeAsync(int followerId, int userId);
    Task<Result<IEnumerable<UserProfileDto>>> GetFriendsAsync(int userId);
}
