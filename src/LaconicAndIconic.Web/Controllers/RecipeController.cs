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
    private readonly IUserService _userService;

    public RecipeController(IRecipeService recipeService, ICategoryService categoryService, IUserService userService)
    {
        _recipeService = recipeService;
        _categoryService = categoryService;
        _userService = userService;
    }

    [HttpGet("Recipe/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (int?)null;
        var result = await _recipeService.GetRecipeByIdAsync(id, currentUserId);
        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        bool isSubscribed = false;
        if (currentUserId.HasValue && currentUserId != result.Value.AuthorId)
        {
            var profileResult = await _userService.GetUserProfileByIdAsync(result.Value.AuthorId, currentUserId);
            if (profileResult.IsSuccess && profileResult.Value != null)
            {
                isSubscribed = profileResult.Value.IsSubscribed;
            }
        }

        var model = new RecipeDetailsViewModel
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Description = result.Value.Description,
            ImagePath = result.Value.ImagePath,
            PrepTimeMin = result.Value.PrepTimeMin,
            Servings = result.Value.Servings,
            Ingredients = result.Value.Ingredients,
            CookingMethod = result.Value.CookingMethod,
            AverageRating = result.Value.AverageRating,
            RatingCount = result.Value.RatingCount,
            CurrentUserRating = result.Value.CurrentUserRating,
            CategoryName = result.Value.CategoryName,
            AuthorId = result.Value.AuthorId,
            AuthorName = result.Value.AuthorName,
            AuthorProfilePicturePath = result.Value.AuthorProfilePicturePath,
            IsSubscribedToAuthor = isSubscribed
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, int score)
    {
        var userId = User.GetUserId();
        var result = await _recipeService.RateRecipeAsync(id, userId, score);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Оцінку збережено";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View(new CreateRecipeDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRecipeDto model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return View(model);
        }

        var userId = User.GetUserId();

        var result = await _recipeService.CreateRecipeAsync(userId, model);
        if (result.IsSuccess)
        {
            return RedirectToAction("Profile", "User", new { id = userId });
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка при створенні рецепту");
        await LoadCategoriesAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.GetUserId();
        var recipeResult = await _recipeService.GetRecipeByIdAsync(id);

        if (!recipeResult.IsSuccess || recipeResult.Value == null)
        {
            return NotFound();
        }

        if (recipeResult.Value.AuthorId != userId)
        {
            return Forbid();
        }

        await LoadCategoriesAsync();

        var model = new EditRecipeViewModel
        {
            Id = recipeResult.Value.Id,
            Title = recipeResult.Value.Title,
            Description = recipeResult.Value.Description,
            PrepTimeMin = recipeResult.Value.PrepTimeMin,
            Servings = recipeResult.Value.Servings,
            Ingredients = recipeResult.Value.Ingredients,
            CookingMethod = recipeResult.Value.CookingMethod,
            CategoryId = recipeResult.Value.CategoryId,
            CurrentImagePath = recipeResult.Value.ImagePath
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditRecipeViewModel model)
    {
        var userId = User.GetUserId();
        var recipeResult = await _recipeService.GetRecipeByIdAsync(id);

        if (!recipeResult.IsSuccess || recipeResult.Value == null)
        {
            return NotFound();
        }

        if (recipeResult.Value.AuthorId != userId)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.CurrentImagePath = recipeResult.Value.ImagePath;
            await LoadCategoriesAsync();
            return View(model);
        }

        var updateDto = new UpdateRecipeDto
        {
            Title = model.Title,
            Description = model.Description,
            PrepTimeMin = model.PrepTimeMin,
            Servings = model.Servings,
            Ingredients = model.Ingredients,
            CookingMethod = model.CookingMethod,
            CategoryId = model.CategoryId,
            Image = model.Image
        };

        var updateResult = await _recipeService.UpdateRecipeAsync(id, userId, updateDto);
        if (updateResult.IsSuccess)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        ModelState.AddModelError(string.Empty, updateResult.ErrorMessage ?? "Помилка при оновленні рецепту");
        model.CurrentImagePath = recipeResult.Value.ImagePath;
        await LoadCategoriesAsync();
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

    private async Task LoadCategoriesAsync()
    {
        var categoryResult = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = categoryResult.IsSuccess ? categoryResult.Value : new List<CategoryDto>();
    }
}
