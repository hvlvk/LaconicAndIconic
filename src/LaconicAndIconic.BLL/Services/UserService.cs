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
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("GetUserProfileByIdAsync: User with ID {Id} was not found", id);
            return Result<UserProfileDto>.Failure("Користувача не знайдено");
        }

        var followerCount = await _subscriptionRepository.CountAsync(s => s.UserId == id);
        var followingCount = await _subscriptionRepository.CountAsync(s => s.FollowerId == id);

        bool isSubscribed = false;
        if (currentUserId.HasValue)
        {
            isSubscribed = await _subscriptionRepository.AnyAsync(s => s.UserId == id && s.FollowerId == currentUserId.Value);
        }

        var dto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            ProfilePicturePath = user.ProfilePicturePath,
            FollowerCount = followerCount,
            FollowingCount = followingCount,
            IsSubscribed = isSubscribed
        };

        _logger.LogInformation("GetUserProfileByIdAsync: Successfully retrieved profile for user ID {Id}", id);
        return Result<UserProfileDto>.Success(dto);
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

        var existingSubscription = await _subscriptionRepository.FindAsync(
            s => s.FollowerId == followerId && s.UserId == userId);

        if (existingSubscription.Any())
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
        var existingSubscription = await _subscriptionRepository.FindAsync(
            s => s.FollowerId == followerId && s.UserId == userId);

        var subscription = existingSubscription.FirstOrDefault();
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

        var users = await _userRepository.FindAsync(u => friendIds.Contains(u.Id));

        var dtos = users.Select(user => new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            ProfilePicturePath = user.ProfilePicturePath
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

        var users = await _userRepository.FindAsync(u => followingUserIds.Contains(u.Id));

        var dtos = new List<UserProfileDto>();
        foreach (var user in users)
        {
            bool isSubscribed = false;
            if (currentUserId.HasValue && currentUserId.Value != userId)
            {
                isSubscribed = await _subscriptionRepository.AnyAsync(s => s.UserId == user.Id && s.FollowerId == currentUserId.Value);
            }

            dtos.Add(new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                ProfilePicturePath = user.ProfilePicturePath,
                FollowerCount = await _subscriptionRepository.CountAsync(s => s.UserId == user.Id),
                FollowingCount = await _subscriptionRepository.CountAsync(s => s.FollowerId == user.Id),
                IsSubscribed = isSubscribed
            });
        }

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

        var users = await _userRepository.FindAsync(u => followerUserIds.Contains(u.Id));

        var dtos = new List<UserProfileDto>();
        foreach (var user in users)
        {
            bool isSubscribed = false;
            if (currentUserId.HasValue && currentUserId.Value != userId)
            {
                isSubscribed = await _subscriptionRepository.AnyAsync(s => s.UserId == user.Id && s.FollowerId == currentUserId.Value);
            }

            dtos.Add(new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                ProfilePicturePath = user.ProfilePicturePath,
                FollowerCount = await _subscriptionRepository.CountAsync(s => s.UserId == user.Id),
                FollowingCount = await _subscriptionRepository.CountAsync(s => s.FollowerId == user.Id),
                IsSubscribed = isSubscribed
            });
        }

        _logger.LogInformation("GetFollowersAsync: Retrieved {Count} followers for user ID {UserId}", dtos.Count, userId);
        return Result<IEnumerable<UserProfileDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<UserProfileDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.FindAsync();
        
        var dtos = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var followerCount = await _subscriptionRepository.CountAsync(s => s.UserId == user.Id);
            var followingCount = await _subscriptionRepository.CountAsync(s => s.FollowerId == user.Id);

            dtos.Add(new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePicturePath = user.ProfilePicturePath,
                FollowerCount = followerCount,
                FollowingCount = followingCount
            });
        }

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

        // Видалення профільної фотографії
        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            _fileService.DeleteFile(user.ProfilePicturePath);
        }

        // Видалення підписок користувача
        var userSubscriptions = await _subscriptionRepository.FindAsync(
            s => s.FollowerId == id || s.UserId == id);
        foreach (var subscription in userSubscriptions)
        {
            _subscriptionRepository.Remove(subscription);
        }

        await _subscriptionRepository.SaveChangesAsync();

        // Видалення користувача
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
