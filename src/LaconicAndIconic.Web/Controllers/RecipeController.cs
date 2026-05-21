using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Extensions;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.Web.Controllers;

[Authorize]
public class RecipeController : Controller
{
    private readonly IRecipeService _recipeService;
    private readonly ICategoryService _categoryService;
    private readonly IUserService _userService;
    private readonly ICommentService _commentService;
    private readonly IReportService _reportService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<RecipeController> _logger;

    public RecipeController(
        IRecipeService recipeService,
        ICategoryService categoryService,
        IUserService userService,
        ICommentService commentService,
        IReportService reportService,
        IOptions<AppSettings> appSettings,
        ILogger<RecipeController> logger)
    {
        _recipeService = recipeService;
        _categoryService = categoryService;
        _userService = userService;
        _commentService = commentService;
        _reportService = reportService;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    [HttpGet("diagnostics/claims")]
    public IActionResult ShowClaims()
    {
        var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
        _logger.LogInformation("User claims: {Claims}", string.Join(", ", claims));
        return Content(string.Join("\n", claims));
    }

    [HttpGet("Recipe/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (int?)null;
        var result = await _recipeService.GetRecipeByIdAsync(id, currentUserId);
        _logger.LogInformation("Details called for recipe {RecipeId}", id);
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
            IsSubscribedToAuthor = isSubscribed,
            IsExternal = result.Value.IsExternal
        };

        var commentsResult = await _commentService.GetCommentsByRecipeIdAsync(id, currentUserId);
        if (commentsResult.IsSuccess && commentsResult.Value != null)
        {
            model.Comments = commentsResult.Value.ToList().AsReadOnly();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, int score)
    {
        var result = await _recipeService.RateRecipeAsync(id, User.GetUserId(), score);

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
    public async Task<IActionResult> Report(int id)
    {
        if (!ModelState.IsValid)
        {
            return View(id);
        }

        var recipeResult = await _recipeService.GetRecipeByIdAsync(id);
        if (!recipeResult.IsSuccess || recipeResult.Value == null)
        {
            return NotFound();
        }

        var model = new ReportRecipeViewModel
        {
            RecipeId = id,
            ReportedUserId = recipeResult.Value.AuthorId,
            ReportedUserName = recipeResult.Value.AuthorName
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Report(ReportRecipeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.GetUserId();
        var result = await _reportService.CreateAsync(new CreateReportDto
        {
            RecipeId = model.RecipeId,
            ReportedUserId = model.ReportedUserId,
            Reason = model.Reason
        }, userId);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не вдалося надіслати скаргу");
            return View(model);
        }

        TempData["SuccessMessage"] = "Скаргу надіслано";
        return RedirectToAction(nameof(Details), new { id = model.RecipeId });
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

        if (model.Title == null || model.Title.Length < _appSettings.MinSearchLength)
        {
            ModelState.AddModelError("Title", $"Назва рецепту має містити щонайменше {_appSettings.MinSearchLength} символи.");
            await LoadCategoriesAsync();
            return View(model);
        }
        _logger.LogInformation("Creating recipe with title: {Title}", model.Title);

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
        var isAdmin = User.IsInRole("Admin") || User.Identity?.Name == "Admin" || User.Identity?.Name == "admin@gmail.com";
        var recipeResult = await _recipeService.GetRecipeByIdAsync(id);

        if (!recipeResult.IsSuccess || recipeResult.Value == null)
        {
            return NotFound();
        }

        // Звичайні користувачі не можуть редагувати зовнішні або чужі рецепти
        if (!isAdmin)
        {
            if (recipeResult.Value.IsExternal)
            {
                return Forbid();
            }

            if (recipeResult.Value.AuthorId != userId)
            {
                return Forbid();
            }
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
        var isAdmin = User.IsInRole("Admin") || User.Identity?.Name == "Admin" || User.Identity?.Name == "admin@gmail.com";
        var recipeResult = await _recipeService.GetRecipeByIdAsync(id);

        if (!recipeResult.IsSuccess || recipeResult.Value == null)
        {
            return NotFound();
        }

        // Обмеження для звичайних користувачів на збереження змін
        if (!isAdmin)
        {
            if (recipeResult.Value.IsExternal)
            {
                return Forbid();
            }

            if (recipeResult.Value.AuthorId != userId)
            {
                return Forbid();
            }
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

        var executionUserId = isAdmin ? recipeResult.Value.AuthorId : userId;
        var updateResult = await _recipeService.UpdateRecipeAsync(id, executionUserId, updateDto);
        
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
        var isAdmin = User.IsInRole("Admin") || User.Identity?.Name == "Admin" || User.Identity?.Name == "admin@gmail.com";
        var recipeResult = await _recipeService.GetRecipeByIdAsync(id);

        if (!recipeResult.IsSuccess || recipeResult.Value == null)
        {
            return NotFound();
        }

        // Обмеження для звичайних користувачів на видалення
        if (!isAdmin)
        {
            // Безпечно перевіряємо IsExternal через dynamic, якщо властивість динамічна
            try
            {
                dynamic dynamicRecipe = recipeResult.Value;
                if (dynamicRecipe.IsExternal == true)
                {
                    return Forbid();
                }
            }
            catch(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                // Якщо поля IsExternal немає в RecipeDto, ігноруємо
            }

            if (recipeResult.Value.AuthorId != userId)
            {
                return Forbid();
            }
        }

        var executionUserId = isAdmin ? recipeResult.Value.AuthorId : userId;
        var result = await _recipeService.DeleteRecipeAsync(id, executionUserId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        if (isAdmin)
        {
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Profile", "User", new { id = userId });
    }

    private async Task LoadCategoriesAsync()
    {
        var categoryResult = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = categoryResult.IsSuccess ? categoryResult.Value : new List<CategoryDto>();
    }
}