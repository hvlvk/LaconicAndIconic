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
    private readonly ICommentService _commentService;

    public RecipeController(IRecipeService recipeService, ICategoryService categoryService, IUserService userService, ICommentService commentService)
    {
        _recipeService = recipeService;
        _categoryService = categoryService;
        _userService = userService;
        _commentService = commentService;
    }

    [HttpGet("Recipe/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _recipeService.GetRecipeByIdAsync(id);
        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        var currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (int?)null;

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
            CategoryName = result.Value.CategoryName,
            AuthorId = result.Value.AuthorId,
            AuthorName = result.Value.AuthorName,
            AuthorProfilePicturePath = result.Value.AuthorProfilePicturePath,
            IsSubscribedToAuthor = isSubscribed
        };

        var commentsResult = await _commentService.GetCommentsByRecipeIdAsync(id);
        if (commentsResult.IsSuccess && commentsResult.Value != null)
        {
            model.Comments = commentsResult.Value.ToList().AsReadOnly();
        }

        return View(model);
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
