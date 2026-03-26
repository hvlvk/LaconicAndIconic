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

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("User/{id:int}")]
    [Authorize]
    public async Task<IActionResult> Profile(int id)
    {
        var result = await _userService.GetUserProfileByIdAsync(id);

        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        var viewModel = new UserProfileViewModel
        {
            Id = result.Value.Id,
            UserName = result.Value.UserName,
            ProfilePicturePath = result.Value.ProfilePicturePath,
            IsOwnProfile = User.GetUserId() == id
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
}
