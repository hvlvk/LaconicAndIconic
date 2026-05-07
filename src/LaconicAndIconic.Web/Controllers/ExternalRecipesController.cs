using LaconicAndIconic.Web.Models;
using LaconicAndIconic.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

public sealed class ExternalRecipesController(IExternalRecipeClient externalRecipeClient) : Controller
{
    [HttpGet("api/external-recipes/search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query is required.");
        }

        var recipes = await externalRecipeClient.SearchRecipesAsync(query, cancellationToken);

        return Ok(recipes);
    }

    [HttpGet("ExternalRecipes/{externalId}")]
    [Authorize]
    public async Task<IActionResult> Details(string externalId, CancellationToken cancellationToken)
    {
        var recipe = await externalRecipeClient.GetRecipeByExternalIdAsync(externalId, cancellationToken);
        if (recipe == null)
        {
            return NotFound();
        }

        var model = new ExternalRecipeDetailsViewModel
        {
            ExternalId = recipe.ExternalId,
            Title = recipe.Title,
            Description = string.Join(", ", new[] { recipe.Category, recipe.Area }.Where(value => !string.IsNullOrWhiteSpace(value))),
            ImagePath = recipe.ThumbnailUri?.ToString(),
            Ingredients = recipe.Ingredients,
            CookingMethod = recipe.Instructions ?? string.Empty,
            CategoryName = recipe.Category ?? "External"
        };

        return View("~/Views/ExternalRecipePages/Details.cshtml", model);
    }
}
