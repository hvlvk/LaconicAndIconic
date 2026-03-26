using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LaconicAndIconic.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFileService _fileService;

    public UserService(IUserRepository userRepository, IFileService fileService)
    {
        _userRepository = userRepository;
        _fileService = fileService;
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
            UserName = user.UserName ?? string.Empty,
            ProfilePicturePath = user.ProfilePicturePath
        };

        return Result<UserProfileDto>.Success(dto);
    }

    public async Task<Result<string>> UpdateProfilePictureAsync(int id, IFormFile file)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
        {
            return Result<string>.Failure("User not found");
        }

        if (file == null || file.Length == 0)
        {
            return Result<string>.Failure("File is empty");
        }

        var newPath = await _fileService.SaveFileAsync(file, "profiles");

        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            _fileService.DeleteFile(user.ProfilePicturePath);
        }

        user.ProfilePicturePath = newPath;
        await _userRepository.UpdateAsync(user);

        return Result<string>.Success(newPath);
    }
}
