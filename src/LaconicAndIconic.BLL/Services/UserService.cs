using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LaconicAndIconic.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFileService _fileService;
    private readonly IRepository<UserSubscription> _subscriptionRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IFileService fileService,
        IRepository<UserSubscription> subscriptionRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _fileService = fileService;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> GetUserProfileByIdAsync(int id, int? currentUserId = null)
    {
        var projection = await _userRepository.GetUserProfileByIdAsync(id, currentUserId);
        if (projection is null)
        {
            _logger.LogWarning("GetUserProfileByIdAsync: User with ID {Id} was not found", id);
            return "Користувача не знайдено";
        }

        var dto = new UserProfileDto
        {
            Id = projection.User.Id,
            UserName = projection.User.UserName ?? string.Empty,
            ProfilePicturePath = projection.User.ProfilePicturePath,
            FollowerCount = projection.FollowerCount,
            FollowingCount = projection.FollowingCount,
            IsSubscribed = projection.IsSubscribed
        };

        _logger.LogInformation("GetUserProfileByIdAsync: Successfully retrieved profile for user ID {Id}", id);
        return dto;
    }

    public async Task<Result<string>> UpdateProfilePictureAsync(int id, IFormFile file)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("UpdateProfilePictureAsync: User with ID {Id} not found", id);
            return Result<string>.Failure("Користувача не знайдено");
        }

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("UpdateProfilePictureAsync: Attempted to upload an empty file for User ID {Id}", id);
            return Result<string>.Failure("Файл порожній");
        }

        var newPath = await _fileService.SaveFileAsync(file, "profiles");

        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            _fileService.DeleteFile(user.ProfilePicturePath);
        }

        user.ProfilePicturePath = newPath;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("UpdateProfilePictureAsync: Successfully updated profile picture for User ID {Id}", id);
        return Result<string>.Success(newPath);
    }

    public async Task<Result> SubscribeAsync(int followerId, int userId)
    {
        if (followerId == userId)
        {
            _logger.LogWarning("SubscribeAsync: User ID {FollowerId} attempted to follow themselves", followerId);
            return Result.Failure("Ви не можете підписатися самі на себе.");
        }

        var userExists = await _userRepository.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            _logger.LogWarning("SubscribeAsync: Target User ID {UserId} was not found", userId);
            return Result.Failure("Користувача для підписки не знайдено.");
        }

        var isAlreadySubscribed = await _subscriptionRepository.AnyAsync(
            s => s.FollowerId == followerId && s.UserId == userId);

        if (isAlreadySubscribed)
        {
            _logger.LogWarning("SubscribeAsync: User ID {FollowerId} is already subscribed to User ID {UserId}", followerId, userId);
            return Result.Failure("Ви вже підписані на цього користувача.");
        }

        var subscription = new UserSubscription
        {
            FollowerId = followerId,
            UserId = userId
        };

        await _subscriptionRepository.AddAsync(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        _logger.LogInformation("SubscribeAsync: User ID {FollowerId} successfully subscribed to User ID {UserId}", followerId, userId);
        return Result.Success();
    }

    public async Task<Result> UnsubscribeAsync(int followerId, int userId)
    {
        var subscription = await _subscriptionRepository.FirstOrDefaultAsync(
            s => s.FollowerId == followerId && s.UserId == userId);
        if (subscription is null)
        {
            _logger.LogWarning("UnsubscribeAsync: Subscription not found for user ID {FollowerId} following User ID {UserId}", followerId, userId);
            return Result.Failure("Ви не підписані на цього користувача.");
        }

        _subscriptionRepository.Remove(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        _logger.LogInformation("UnsubscribeAsync: User ID {FollowerId} successfully unsubscribed from User ID {UserId}", followerId, userId);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<UserProfileDto>>> GetFriendsAsync(int userId)
    {
        var subscriptions = await _subscriptionRepository.FindAsync(s => s.FollowerId == userId);
        var friendIds = subscriptions.Select(s => s.UserId).ToList();

        if (friendIds.Count == 0)
        {
            return Result<IEnumerable<UserProfileDto>>.Success(new List<UserProfileDto>());
        }

        var projections = await _userRepository.GetUserProfilesAsync(friendIds);

        var dtos = projections.Select(p => new UserProfileDto
        {
            Id = p.User.Id,
            UserName = p.User.UserName ?? string.Empty,
            ProfilePicturePath = p.User.ProfilePicturePath,
            FollowerCount = p.FollowerCount,
            FollowingCount = p.FollowingCount,
            IsSubscribed = p.IsSubscribed
        }).ToList();

        return Result<IEnumerable<UserProfileDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<UserProfileDto>>> GetSubscriptionsAsync(int userId, int? currentUserId = null)
    {
        var subscriptions = await _subscriptionRepository.FindAsync(s => s.FollowerId == userId);
        var followingUserIds = subscriptions.Select(s => s.UserId).ToList();

        if (followingUserIds.Count == 0)
        {
            return Result<IEnumerable<UserProfileDto>>.Success(new List<UserProfileDto>());
        }

        var projections = await _userRepository.GetUserProfilesAsync(followingUserIds, currentUserId);

        var dtos = projections.Select(p => new UserProfileDto
        {
            Id = p.User.Id,
            UserName = p.User.UserName ?? string.Empty,
            ProfilePicturePath = p.User.ProfilePicturePath,
            FollowerCount = p.FollowerCount,
            FollowingCount = p.FollowingCount,
            IsSubscribed = p.IsSubscribed
        }).ToList();

        _logger.LogInformation("GetSubscriptionsAsync: Retrieved {Count} subscriptions for user ID {UserId}", dtos.Count, userId);
        return Result<IEnumerable<UserProfileDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<UserProfileDto>>> GetFollowersAsync(int userId, int? currentUserId = null)
    {
        var subscriptions = await _subscriptionRepository.FindAsync(s => s.UserId == userId);
        var followerUserIds = subscriptions.Select(s => s.FollowerId).ToList();

        if (followerUserIds.Count == 0)
        {
            return Result<IEnumerable<UserProfileDto>>.Success(new List<UserProfileDto>());
        }

        var projections = await _userRepository.GetUserProfilesAsync(followerUserIds, currentUserId);

        var dtos = projections.Select(p => new UserProfileDto
        {
            Id = p.User.Id,
            UserName = p.User.UserName ?? string.Empty,
            ProfilePicturePath = p.User.ProfilePicturePath,
            FollowerCount = p.FollowerCount,
            FollowingCount = p.FollowingCount,
            IsSubscribed = p.IsSubscribed
        }).ToList();

        _logger.LogInformation("GetFollowersAsync: Retrieved {Count} followers for user ID {UserId}", dtos.Count, userId);
        return Result<IEnumerable<UserProfileDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<UserProfileDto>>> GetAllUsersAsync()
    {
        var projections = await _userRepository.GetUserProfilesAsync();
        
        var dtos = projections.Select(p => new UserProfileDto
        {
            Id = p.User.Id,
            UserName = p.User.UserName ?? string.Empty,
            Email = p.User.Email ?? string.Empty,
            ProfilePicturePath = p.User.ProfilePicturePath,
            FollowerCount = p.FollowerCount,
            FollowingCount = p.FollowingCount,
            IsSubscribed = p.IsSubscribed
        }).ToList();

        _logger.LogInformation("GetAllUsersAsync: Retrieved {Count} users", dtos.Count);
        return Result<IEnumerable<UserProfileDto>>.Success(dtos);
    }

    public async Task<Result> DeleteUserAsync(int id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("DeleteUserAsync: User with ID {Id} was not found", id);
            return Result.Failure("Користувача не знайдено");
        }

        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            _fileService.DeleteFile(user.ProfilePicturePath);
        }

        var userSubscriptions = await _subscriptionRepository.FindAsync(
            s => s.FollowerId == id || s.UserId == id);
        foreach (var subscription in userSubscriptions)
        {
            _subscriptionRepository.Remove(subscription);
        }

        await _subscriptionRepository.SaveChangesAsync();

        var result = await _userRepository.DeleteAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("DeleteUserAsync: Failed to delete user ID {Id}", id);
            return Result.Failure("Не вдалося видалити користувача");
        }

        _logger.LogInformation("DeleteUserAsync: Successfully deleted user ID {Id}", id);
        return Result.Success();
    }
}
