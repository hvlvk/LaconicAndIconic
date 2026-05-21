using System.Diagnostics;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaconicAndIconic.Web.Filters;
using LaconicAndIconic.Web.Services;

namespace LaconicAndIconic.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRecipeService _recipeService;
    private readonly ICategoryService _categoryService;
    private readonly IExternalRecipeClient _externalRecipeClient;

    public HomeController(
        ILogger<HomeController> logger,
        IRecipeService recipeService,
        ICategoryService categoryService,
        IExternalRecipeClient externalRecipeClient)
    {
        _logger = logger;
        _recipeService = recipeService;
        _categoryService = categoryService;
        _externalRecipeClient = externalRecipeClient;
    }

    [HttpGet]
    [AllowAnonymous]
    [RateLimiting(5)]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? externalSearchTerm,
        int? categoryId,
        string? sortBy,
        int pageNumber = 1)
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
        var externalRecipes = await GetExternalRecipesAsync(externalSearchTerm, HttpContext.RequestAborted);

        var viewModel = new RecipeListViewModel
        {
            Recipes = result.IsSuccess && result.Value?.Recipes is not null ? result.Value.Recipes : [],
            ExternalRecipes = externalRecipes,
            TotalCount = result.IsSuccess && result.Value is not null ? result.Value.TotalCount : 0,
            PageNumber = result.IsSuccess && result.Value is not null ? result.Value.PageNumber : pageNumber,
            PageSize = result.IsSuccess && result.Value is not null ? result.Value.PageSize : 10,
            SearchTerm = searchTerm ?? string.Empty,
            ExternalSearchTerm = externalSearchTerm ?? string.Empty,
            CategoryId = categoryId,
            SortBy = sortBy  ?? string.Empty,
            Categories = categories
        };

        ViewBag.Categories = categories;
        return View(viewModel);
    }

    private async Task<IReadOnlyList<RecipeDto>> GetExternalRecipesAsync(
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        try
        {
            var externalRecipes = await _externalRecipeClient.SearchRecipesAsync(searchTerm, cancellationToken);
            return externalRecipes.Select(recipe => new RecipeDto
            {
                Id = 0,
                Title = recipe.Title,
                Description = string.Join(", ", new[] { recipe.Category, recipe.Area }.Where(value => !string.IsNullOrWhiteSpace(value))),
                ImagePath = recipe.ThumbnailUri?.ToString(),
                PrepTimeMin = 30,
                Servings = 1,
                Ingredients = recipe.Ingredients,
                CookingMethod = recipe.Instructions ?? string.Empty,
                CategoryName = recipe.Category ?? "External",
                AuthorName = "TheMealDB",
                IsExternal = true,
                ExternalSource = "TheMealDB",
                ExternalId = recipe.ExternalId
            }).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "External recipe search failed for {SearchTerm}", searchTerm);
            return [];
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "External recipe search timed out for {SearchTerm}", searchTerm);
            return [];
        }
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

    [HttpGet]
    public IActionResult RateLimitError()
    {
        return View();
    }
}
