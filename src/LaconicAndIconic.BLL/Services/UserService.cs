using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Interfaces;

namespace LaconicAndIconic.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserProfileDto>> GetUserProfileByIdAsync(int id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
        {
            return Result<UserProfileDto>.Failure("User not found");
        }

        var dto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty
        };

        return Result<UserProfileDto>.Success(dto);
    }
}
