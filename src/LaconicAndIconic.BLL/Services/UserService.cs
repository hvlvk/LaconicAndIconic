using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LaconicAndIconic.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFileService _fileService;
    private readonly IRepository<UserSubscription> _subscriptionRepository;

    public UserService(
        IUserRepository userRepository, 
        IFileService fileService,
        IRepository<UserSubscription> subscriptionRepository)
    {
        _userRepository = userRepository;
        _fileService = fileService;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<UserProfileDto>> GetUserProfileByIdAsync(int id, int? currentUserId = null)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
        {
            return Result<UserProfileDto>.Failure("User not found");
        }

        var followers = await _subscriptionRepository.FindAsync(s => s.UserId == id);
        var following = await _subscriptionRepository.FindAsync(s => s.FollowerId == id);
        
        bool isSubscribed = false;
        if (currentUserId.HasValue)
        {
            isSubscribed = followers.Any(s => s.FollowerId == currentUserId.Value);
        }

        var dto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            ProfilePicturePath = user.ProfilePicturePath,
            FollowerCount = followers.Count(),
            FollowingCount = following.Count(),
            IsSubscribed = isSubscribed
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

    public async Task<Result> SubscribeAsync(int followerId, int userId)
    {
        if (followerId == userId)
        {
            return Result.Failure("Users cannot follow themselves.");
        }

        var userExists = await _userRepository.FindByIdAsync(userId);
        if (userExists is null)
        {
            return Result.Failure("User to follow not found.");
        }

        var existingSubscription = await _subscriptionRepository.FindAsync(
            s => s.FollowerId == followerId && s.UserId == userId);

        if (existingSubscription.Any())
        {
            return Result.Failure("Already followed.");
        }

        var subscription = new UserSubscription
        {
            FollowerId = followerId,
            UserId = userId
        };

        await _subscriptionRepository.AddAsync(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UnsubscribeAsync(int followerId, int userId)
    {
        var existingSubscription = await _subscriptionRepository.FindAsync(
            s => s.FollowerId == followerId && s.UserId == userId);

        var subscription = existingSubscription.FirstOrDefault();
        if (subscription is null)
        {
            return Result.Failure("Not followed.");
        }

        _subscriptionRepository.Remove(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        return Result.Success();
    }
}
