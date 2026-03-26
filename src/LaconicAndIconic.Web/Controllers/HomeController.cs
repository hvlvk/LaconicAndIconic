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

    public HomeController(ILogger<HomeController> logger, IRecipeService recipeService)
    {
        _logger = logger;
        _recipeService = recipeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _recipeService.GetAllRecipesAsync();
        var model = result.IsSuccess && result.Value is not null
            ? result.Value
            : Enumerable.Empty<RecipeDto>();
        return View(model);
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
