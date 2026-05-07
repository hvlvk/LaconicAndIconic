namespace LaconicAndIconic.BLL.Interfaces;

/// <summary>
/// Interface for cache invalidation across services.
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates all categories cache.
    /// </summary>
    void InvalidateCategoriesCache();

    /// <summary>
    /// Invalidates all recipes cache.
    /// </summary>
    void InvalidateRecipesCache();

    /// <summary>
    /// Invalidates cache for a specific recipe by ID.
    /// </summary>
    void InvalidateRecipeCache(int recipeId);

    /// <summary>
    /// Invalidates cache for recipes by a specific author.
    /// </summary>
    void InvalidateAuthorRecipesCache(int authorId);

    /// <summary>
    /// Invalidates cache for ratings of a specific recipe.
    /// </summary>
    void InvalidateRecipeRatingsCache(int recipeId);
}
