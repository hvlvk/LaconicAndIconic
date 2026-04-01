using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Extensions;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

[Authorize]
public class RecipeController : Controller
{
    private readonly IRecipeService _recipeService;
    private readonly ICategoryService _categoryService;
    private readonly IFavoriteService _favoriteService;

    public RecipeController(IRecipeService recipeService, ICategoryService categoryService, IFavoriteService favoriteService)
    {
        _recipeService = recipeService;
        _categoryService = categoryService;
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _recipeService.GetRecipeByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        var isFavorited = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            isFavorited = await _favoriteService.IsFavoriteAsync(User.GetUserId(), id);
        }

        return View(new RecipeDetailsViewModel
        {
            Recipe = result.Value!,
            IsFavorited = isFavorited
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categoryResult = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = categoryResult.IsSuccess ? categoryResult.Value : new List<CategoryDto>();
        return View(new CreateRecipeDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRecipeDto model)
    {
        if (!ModelState.IsValid)
        {
            var categoryResult = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categoryResult.IsSuccess ? categoryResult.Value : new List<CategoryDto>();
            return View(model);
        }

        var userId = User.GetUserId();

        var result = await _recipeService.CreateRecipeAsync(userId, model);
        if (result.IsSuccess)
        {
            return RedirectToAction("Profile", "User", new { id = userId });
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка при створенні рецепту");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var result = await _recipeService.DeleteRecipeAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction("Profile", "User", new { id = userId });
    }
}
