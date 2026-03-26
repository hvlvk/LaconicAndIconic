using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IUserService
{
    Task<Result<UserProfileDto>> GetUserProfileByIdAsync(int id);
}
