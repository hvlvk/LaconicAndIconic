using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IUserService _userService;

    public AdminController(ICategoryService categoryService, IUserService userService)
    {
        _categoryService = categoryService;
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new AdminMenuViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> ManageCategories()
    {
        var categoriesResult = await _categoryService.GetAllAsync();
        var categories = categoriesResult.IsSuccess && categoriesResult.Value is not null
            ? categoriesResult.Value.ToList()
            : new List<CategoryDto>();

        var model = new AdminCategoriesViewModel
        {
            Categories = categories.AsReadOnly()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        var usersResult = await _userService.GetAllUsersAsync();
        var users = usersResult.IsSuccess && usersResult.Value is not null
            ? usersResult.Value.ToList()
            : new List<UserProfileDto>();

        var model = new AdminUsersViewModel
        {
            Users = users.AsReadOnly()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося видалити користувача.";
        }
        else
        {
            TempData["SuccessMessage"] = "Користувача успішно видалено.";
        }

        return RedirectToAction(nameof(ManageUsers));
    }

    // Категорії методи
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Вкажіть назву категорії.";
            return RedirectToAction(nameof(ManageCategories));
        }

        var result = await _categoryService.CreateAsync(name);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося додати категорію.";
        }
        else
        {
            TempData["SuccessMessage"] = "Категорію успішно додано.";
        }

        return RedirectToAction(nameof(ManageCategories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(int id, string name)
    {
        var result = await _categoryService.UpdateAsync(id, name);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося оновити категорію.";
        }
        else
        {
            TempData["SuccessMessage"] = "Категорію успішно оновлено.";
        }

        return RedirectToAction(nameof(ManageCategories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося видалити категорію.";
        }
        else
        {
            TempData["SuccessMessage"] = "Категорію успішно видалено.";
        }

        return RedirectToAction(nameof(ManageCategories));
    }
}
