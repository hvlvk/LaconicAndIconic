using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

[Authorize]
public class CommentController : Controller
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Перевірте правильність введених даних";
            return RedirectToAction("Details", "Recipe", new { id = dto.RecipeId });
        }

        var userId = User.GetUserId();
        var result = await _commentService.CreateAsync(dto, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Помилка при додаванні коментаря";
        }

        return RedirectToAction("Details", "Recipe", new { id = dto.RecipeId });
    }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id, int recipeId, bool isFromServerAdmin = false)
{
    var userId = User.GetUserId();
    
    // Якщо форма принесла true (бо ми захардкодили його для адміна), або .NET розпізнав роль
    var isUserAdmin = isFromServerAdmin || User.IsInRole("Admin") || User.Identity?.Name == "Admin" || User.Identity?.Name == "admin@gmail.com";

    // Якщо це адмін — передаємо 0 (наш чарівний ключ доступу), якщо ні — передаємо реальний userId
    var executionUserId = isUserAdmin ? 0 : userId;

    var result = await _commentService.DeleteAsync(id, executionUserId);

    if (!result.IsSuccess)
    {
        TempData["ErrorMessage"] = result.ErrorMessage;
    }

    return RedirectToAction("Details", "Recipe", new { id = recipeId });
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Перевірте правильність введених даних";
            return RedirectToAction("Details", "Recipe", new { id = dto.RecipeId });
        }

        var userId = User.GetUserId();
        var result = await _commentService.EditAsync(dto, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Помилка при редагуванні коментаря";
        }

        return RedirectToAction("Details", "Recipe", new { id = dto.RecipeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike(int id, int recipeId)
    {
        var userId = User.GetUserId();
        var result = await _commentService.ToggleLikeAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Помилка при лайканні коментаря";
        }

        return RedirectToAction("Details", "Recipe", new { id = recipeId });
    }
}
