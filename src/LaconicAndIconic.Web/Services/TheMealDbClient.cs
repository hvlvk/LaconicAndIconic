using System.Net.Http.Json;
using System.Text.Json;
using LaconicAndIconic.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.Web.Services;

public sealed class TheMealDbClient(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IOptions<TheMealDbOptions> options,
    ILogger<TheMealDbClient> logger) : IExternalRecipeClient
{
    private const string CacheKeyPrefix = "external_recipes_themealdb_";

    public async Task<IReadOnlyCollection<ExternalRecipeDto>> SearchRecipesAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var cacheKey = $"{CacheKeyPrefix}search_{searchTerm.Trim().ToUpperInvariant()}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<ExternalRecipeDto>? cachedRecipes))
        {
            return cachedRecipes!;
        }

        var requestUri = $"search.php?s={Uri.EscapeDataString(searchTerm.Trim())}";
        var payload = await SendAsync(requestUri, searchTerm, cancellationToken);
        var recipes = MapMeals(payload?.Meals).ToList().AsReadOnly();

        memoryCache.Set(cacheKey, recipes, TimeSpan.FromMinutes(options.Value.CacheMinutes));

        return recipes;
    }

    public async Task<ExternalRecipeDto?> GetRecipeByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            return null;
        }

        var cacheKey = $"{CacheKeyPrefix}lookup_{externalId.Trim()}";
        if (memoryCache.TryGetValue(cacheKey, out ExternalRecipeDto? cachedRecipe))
        {
            return cachedRecipe;
        }

        var requestUri = $"lookup.php?i={Uri.EscapeDataString(externalId.Trim())}";
        var payload = await SendAsync(requestUri, externalId, cancellationToken);
        var recipe = MapMeals(payload?.Meals).FirstOrDefault();

        if (recipe != null)
        {
            memoryCache.Set(cacheKey, recipe, TimeSpan.FromMinutes(options.Value.CacheMinutes));
        }

        return recipe;
    }

    private async Task<TheMealDbSearchResponse?> SendAsync(
        string requestUri,
        string logScope,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "TheMealDB request failed with status code {StatusCode} for search term {SearchTerm}",
                response.StatusCode,
                logScope);
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TheMealDbSearchResponse>(
            cancellationToken: cancellationToken);
    }

    private static IEnumerable<ExternalRecipeDto> MapMeals(IReadOnlyCollection<TheMealDbMeal>? meals)
    {
        if (meals is null)
        {
            return [];
        }

        return meals
            .Where(meal => !string.IsNullOrWhiteSpace(meal.IdMeal) && !string.IsNullOrWhiteSpace(meal.StrMeal))
            .Select(meal => new ExternalRecipeDto(
                meal.IdMeal,
                meal.StrMeal,
                meal.StrCategory,
                meal.StrArea,
                meal.StrInstructions,
                FormatIngredients(meal),
                CreateUriOrNull(meal.StrMealThumb),
                CreateUriOrNull(meal.StrSource)));
    }

    private static Uri? CreateUriOrNull(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
    }

    private static string FormatIngredients(TheMealDbMeal meal)
    {
        var ingredients = Enumerable.Range(1, 20)
            .Select(index =>
            {
                var ingredient = GetAdditionalString(meal, $"strIngredient{index}");
                if (string.IsNullOrWhiteSpace(ingredient))
                {
                    return null;
                }

                var measure = GetAdditionalString(meal, $"strMeasure{index}");
                return string.IsNullOrWhiteSpace(measure)
                    ? ingredient.Trim()
                    : $"{measure.Trim()} {ingredient.Trim()}";
            })
            .Where(item => !string.IsNullOrWhiteSpace(item));

        return string.Join(Environment.NewLine, ingredients);
    }

    private static string? GetAdditionalString(TheMealDbMeal meal, string key)
    {
        if (!meal.AdditionalFields.TryGetValue(key, out var value))
        {
            return null;
        }

        if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
        {
            return jsonElement.GetString();
        }

        return value?.ToString();
    }
}
