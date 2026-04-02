using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
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

    public UserController(IUserService userService, IRecipeService recipeService)
    {
        _userService = userService;
        _recipeService = recipeService;
    }

    [HttpGet("User/{id:int}")]
    [Authorize]
    public async Task<IActionResult> Profile(int id)
    {
        var currentUserId = User.GetUserId();
        var result = await _userService.GetUserProfileByIdAsync(id, currentUserId);

        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        var recipesResult = await _recipeService.GetRecipesByAuthorIdAsync(id);

        var viewModel = new UserProfileViewModel
        {
            Id = result.Value.Id,
            UserName = result.Value.UserName,
            ProfilePicturePath = result.Value.ProfilePicturePath,
            IsOwnProfile = currentUserId == id,
            FollowerCount = result.Value.FollowerCount,
            FollowingCount = result.Value.FollowingCount,
            IsSubscribed = result.Value.IsSubscribed,
            Recipes = recipesResult.IsSuccess && recipesResult.Value != null ? recipesResult.Value : []
        };

        return View(viewModel);
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
    public async Task<IActionResult> Subscribe(int id)
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

        return RedirectToAction(nameof(Profile), new { id });
    }

    [HttpPost("User/{id:int}/Unsubscribe")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe(int id)
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

        return RedirectToAction(nameof(Profile), new { id });
    }
}
