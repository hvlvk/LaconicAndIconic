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
    public async Task<IActionResult> Delete(int id, int recipeId)
    {
        var userId = User.GetUserId();
        var result = await _commentService.DeleteAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Помилка при видаленні коментаря";
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
}
