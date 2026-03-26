using System.Threading.Tasks;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.Web.Extensions;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
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
            UserName = result.Value.UserName,
            IsOwnProfile = User.GetUserId() == id
        };

        return View("~/Views/Home/UserProfile.cshtml", viewModel);
    }
}
