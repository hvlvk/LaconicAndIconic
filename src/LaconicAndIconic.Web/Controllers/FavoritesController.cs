using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _favoriteService.GetFavoritesByUserAsync(User.GetUserId());
        IEnumerable<RecipeDto> recipes = result.IsSuccess ? result.Value! : [];
        return View(recipes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int recipeId)
    {
        await _favoriteService.AddFavoriteAsync(User.GetUserId(), recipeId);
        return RedirectToAction("Details", "Recipe", new { id = recipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int recipeId)
    {
        await _favoriteService.RemoveFavoriteAsync(User.GetUserId(), recipeId);
        return RedirectToAction("Details", "Recipe", new { id = recipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int recipeId)
    {
        var isFavorited = await _favoriteService.IsFavoriteAsync(User.GetUserId(), recipeId);
        if (isFavorited)
        {
            await _favoriteService.RemoveFavoriteAsync(User.GetUserId(), recipeId);
        }
        else
        {
            await _favoriteService.AddFavoriteAsync(User.GetUserId(), recipeId);
        }

        return Json(new { favorited = !isFavorited });
    }
}
