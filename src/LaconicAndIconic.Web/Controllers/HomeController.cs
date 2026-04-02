using System.Diagnostics;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRecipeService _recipeService;
    private readonly ICategoryService _categoryService;

    public HomeController(ILogger<HomeController> logger, IRecipeService recipeService, ICategoryService categoryService)
    {
        _logger = logger;
        _recipeService = recipeService;
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? sortBy, int pageNumber = 1)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        var result = await _recipeService.SearchRecipesAsync(new RecipeSearchFilterDto
        {
            SearchTerm = searchTerm ?? string.Empty,
            CategoryId = categoryId,
            SortBy = sortBy ?? string.Empty,
            PageNumber = pageNumber,
            PageSize = 10
        });

        var categoriesResult = await _categoryService.GetAllCategoriesAsync();
        var categories = categoriesResult.IsSuccess && categoriesResult.Value is not null
            ? categoriesResult.Value.ToList()
            : [];

        var viewModel = new RecipeListViewModel
        {
            Recipes = result.IsSuccess && result.Value?.Recipes is not null ? result.Value.Recipes : [],
            TotalCount = result.IsSuccess && result.Value is not null ? result.Value.TotalCount : 0,
            PageNumber = result.IsSuccess && result.Value is not null ? result.Value.PageNumber : pageNumber,
            PageSize = result.IsSuccess && result.Value is not null ? result.Value.PageSize : 10,
            SearchTerm = searchTerm ?? string.Empty,
            CategoryId = categoryId,
            SortBy = sortBy  ?? string.Empty,
            Categories = categories
        };

        ViewBag.Categories = categories;
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
