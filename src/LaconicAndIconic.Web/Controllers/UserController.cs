using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Extensions;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IRecipeService _recipeService;
    private readonly ISharedListService _sharedListService;

    public UserController(
        IUserService userService,
        IRecipeService recipeService,
        ISharedListService sharedListService)
    {
        _userService = userService;
        _recipeService = recipeService;
        _sharedListService = sharedListService;
    }

    [HttpGet("User/{id:int}")]
    [Authorize]
    public async Task<IActionResult> Profile(int id, string tab = "recipes", int? sharedListId = null)
    {
        var currentUserId = User.GetUserId();
        var result = await _userService.GetUserProfileByIdAsync(id, currentUserId);

        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        var recipesResult = await _recipeService.GetRecipesByAuthorIdAsync(id);
        var isOwnProfile = currentUserId == id;
        var activeTab = "recipes";

        if (isOwnProfile && string.Equals(tab, "lists", StringComparison.OrdinalIgnoreCase))
        {
            activeTab = "lists";
        }
        else if (isOwnProfile && string.Equals(tab, "saved", StringComparison.OrdinalIgnoreCase))
        {
            activeTab = "saved";
        }

        var viewModel = new UserProfileViewModel
        {
            Id = result.Value.Id,
            UserName = result.Value.UserName,
            ProfilePicturePath = result.Value.ProfilePicturePath,
            IsOwnProfile = isOwnProfile,
            FollowerCount = result.Value.FollowerCount,
            FollowingCount = result.Value.FollowingCount,
            IsSubscribed = result.Value.IsSubscribed,
            ActiveTab = activeTab,
            Recipes = recipesResult.IsSuccess && recipesResult.Value != null ? recipesResult.Value : []
        };

        if (isOwnProfile)
        {
            var friendsResult = await _userService.GetFriendsAsync(id);
            viewModel.Friends = friendsResult.IsSuccess && friendsResult.Value != null ? friendsResult.Value : [];

            var allRecipesResult = await _recipeService.GetAllRecipesAsync();
            viewModel.AllRecipes = allRecipesResult.IsSuccess && allRecipesResult.Value != null ? allRecipesResult.Value : [];

            // Load saved recipes
            var savedRecipesResult = await _recipeService.GetSavedRecipesByUserIdAsync(id);
            viewModel.SavedRecipes = savedRecipesResult.IsSuccess && savedRecipesResult.Value != null ? savedRecipesResult.Value : [];

            var sharedListsResult = await _sharedListService.GetListsByUserAsync(id);
            if (sharedListsResult.IsSuccess && sharedListsResult.Value != null)
            {
                var sharedLists = sharedListsResult.Value
                    .Select(ToSharedListViewModel)
                    .ToList();

                viewModel.SharedLists = sharedLists;

                if (activeTab == "lists")
                {
                    int? selectedListId = sharedListId;
                    if (!selectedListId.HasValue && sharedLists.Count > 0)
                    {
                        selectedListId = sharedLists[0].Id;
                    }

                    if (selectedListId.HasValue)
                    {
                        var selectedListResult = await _sharedListService.GetByIdAsync(selectedListId.Value, id);
                        if (selectedListResult.IsSuccess && selectedListResult.Value != null)
                        {
                            viewModel.SelectedSharedList = ToSharedListDetailViewModel(selectedListResult.Value, id);
                        }
                        else if (!string.IsNullOrWhiteSpace(selectedListResult.ErrorMessage))
                        {
                            TempData["ErrorMessage"] = selectedListResult.ErrorMessage;
                        }
                    }
                }
            }
            else if (activeTab == "lists" && !string.IsNullOrWhiteSpace(sharedListsResult.ErrorMessage))
            {
                TempData["ErrorMessage"] = sharedListsResult.ErrorMessage;
            }
        }

        return View(viewModel);
    }

    private static SharedListViewModel ToSharedListViewModel(SharedListDto dto)
    {
        return new SharedListViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            OwnerName = dto.OwnerName,
            MemberCount = dto.MemberCount,
            RecipeCount = dto.RecipeCount
        };
    }

    private static SharedListDetailViewModel ToSharedListDetailViewModel(SharedListDetailDto dto, int currentUserId)
    {
        return new SharedListDetailViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            OwnerId = dto.OwnerId,
            OwnerName = dto.OwnerName,
            IsOwner = dto.OwnerId == currentUserId,
            Members = dto.Members,
            Recipes = dto.Recipes
        };
    }

    [HttpPost("User/UpdateProfilePicture")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
    {
        var userId = User.GetUserId();

        var result = await _userService.UpdateProfilePictureAsync(userId, profilePicture);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Profile), new { id = userId });
    }

    [HttpPost("User/{id:int}/Subscribe")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(int id, string? returnUrl = null)
    {
        var followerId = User.GetUserId();

        var result = await _userService.SubscribeAsync(followerId, id);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Ви успішно підписалися!";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Profile), new { id });
    }

    [HttpPost("User/{id:int}/Unsubscribe")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe(int id, string? returnUrl = null)
    {
        var followerId = User.GetUserId();

        var result = await _userService.UnsubscribeAsync(followerId, id);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Ви успішно відписалися!";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Profile), new { id });
    }

    [HttpGet("User/{id:int}/Subscriptions")]
    [Authorize]
    public async Task<IActionResult> Subscriptions(int id)
    {
        var currentUserId = User.GetUserId();
        var userResult = await _userService.GetUserProfileByIdAsync(id, currentUserId);

        if (!userResult.IsSuccess || userResult.Value == null)
        {
            return NotFound();
        }

        var subscriptionsResult = await _userService.GetSubscriptionsAsync(id, currentUserId);

        var viewModel = new UserListViewModel
        {
            UserId = id,
            UserName = userResult.Value.UserName,
            ListType = "subscriptions",
            IsOwnList = currentUserId == id,
            Users = subscriptionsResult.IsSuccess && subscriptionsResult.Value != null ? subscriptionsResult.Value : []
        };

        return View(viewModel);
    }

    [HttpGet("User/{id:int}/Followers")]
    [Authorize]
    public async Task<IActionResult> Followers(int id)
    {
        var currentUserId = User.GetUserId();
        var userResult = await _userService.GetUserProfileByIdAsync(id, currentUserId);

        if (!userResult.IsSuccess || userResult.Value == null)
        {
            return NotFound();
        }

        var followersResult = await _userService.GetFollowersAsync(id, currentUserId);

        var viewModel = new UserListViewModel
        {
            UserId = id,
            UserName = userResult.Value.UserName,
            ListType = "followers",
            IsOwnList = currentUserId == id,
            Users = followersResult.IsSuccess && followersResult.Value != null ? followersResult.Value : []
        };

        return View(viewModel);
    }
}
