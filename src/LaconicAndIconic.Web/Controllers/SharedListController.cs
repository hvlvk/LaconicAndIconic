using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Extensions;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

[Authorize]
public class SharedListController : Controller
{
    private readonly ISharedListService _sharedListService;

    public SharedListController(ISharedListService sharedListService)
    {
        _sharedListService = sharedListService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var result = await _sharedListService.GetListsByUserAsync(userId);

        if (!result.IsSuccess || result.Value == null)
        {
            return View(Enumerable.Empty<SharedListViewModel>());
        }

        var viewModels = result.Value.Select(dto => new SharedListViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            OwnerName = dto.OwnerName,
            MemberCount = dto.MemberCount,
            RecipeCount = dto.RecipeCount
        });

        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.GetUserId();
        var result = await _sharedListService.GetByIdAsync(id, userId);

        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        var viewModel = new SharedListDetailViewModel
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Description = result.Value.Description,
            OwnerId = result.Value.OwnerId,
            OwnerName = result.Value.OwnerName,
            IsOwner = result.Value.OwnerId == userId,
            Members = result.Value.Members,
            Recipes = result.Value.Recipes
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateSharedListViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSharedListViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.GetUserId();
        var dto = new CreateSharedListDto
        {
            Title = model.Title,
            Description = model.Description
        };

        var result = await _sharedListService.CreateAsync(dto, userId);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка створення списку");
            return View(model);
        }

        TempData["SuccessMessage"] = "Список створено";
        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.GetUserId();
        var result = await _sharedListService.GetByIdAsync(id, userId);

        if (!result.IsSuccess || result.Value == null)
        {
            return NotFound();
        }

        if (result.Value.OwnerId != userId)
        {
            return Forbid();
        }

        var viewModel = new EditSharedListViewModel
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Description = result.Value.Description
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditSharedListViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.GetUserId();
        var dto = new UpdateSharedListDto
        {
            Title = model.Title,
            Description = model.Description
        };

        var result = await _sharedListService.UpdateAsync(model.Id, userId, dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка оновлення списку");
            return View(model);
        }

        TempData["SuccessMessage"] = "Список оновлено";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var result = await _sharedListService.DeleteAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Список видалено";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteUser(int id, string username)
    {
        var userId = User.GetUserId();
        var result = await _sharedListService.InviteUserAsync(id, userId, username);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Користувача запрошено";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(int id, int userId)
    {
        var currentUserId = User.GetUserId();
        var result = await _sharedListService.RemoveUserAsync(id, currentUserId, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Учасника видалено";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRecipe(int id, int recipeId)
    {
        var userId = User.GetUserId();
        var result = await _sharedListService.AddRecipeAsync(id, userId, recipeId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Рецепт додано до списку";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRecipe(int id, int recipeId)
    {
        var userId = User.GetUserId();
        var result = await _sharedListService.RemoveRecipeAsync(id, userId, recipeId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Рецепт видалено зі списку";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
