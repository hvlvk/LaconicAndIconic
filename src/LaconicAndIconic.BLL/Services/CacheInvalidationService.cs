using Microsoft.Extensions.Caching.Memory;
using LaconicAndIconic.BLL.Interfaces;

namespace LaconicAndIconic.BLL.Services;

/// <summary>
/// Service for managing cache invalidation across the application.
/// </summary>
public class CacheInvalidationService : ICacheInvalidationService
{
    private const string AllCategoriesCacheKey = "all_categories";
    private const string AllRecipesCacheKey = "all_recipes";
    private const string RecipeCacheKeyPrefix = "recipe_";
    private const string AuthorRecipesCacheKeyPrefix = "author_recipes_";
    private const string RecipeRatingsCacheKeyPrefix = "recipe_ratings_";

    private readonly IMemoryCache _memoryCache;

    public CacheInvalidationService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public void InvalidateCategoriesCache()
    {
        _memoryCache.Remove(AllCategoriesCacheKey);
    }

    public void InvalidateRecipesCache()
    {
        _memoryCache.Remove(AllRecipesCacheKey);
    }

    public void InvalidateRecipeCache(int recipeId)
    {
        _memoryCache.Remove($"{RecipeCacheKeyPrefix}{recipeId}");
    }

    public void InvalidateAuthorRecipesCache(int authorId)
    {
        _memoryCache.Remove($"{AuthorRecipesCacheKeyPrefix}{authorId}");
    }

    public void InvalidateRecipeRatingsCache(int recipeId)
    {
        _memoryCache.Remove($"{RecipeRatingsCacheKeyPrefix}{recipeId}");
    }
}
