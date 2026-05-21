namespace LaconicAndIconic.BLL.Interfaces;

public interface ICacheInvalidationService
{
    void InvalidateCategoriesCache();

    void InvalidateRecipesCache();

    void InvalidateRecipeCache(int recipeId);

    void InvalidateAuthorRecipesCache(int authorId);

    void InvalidateRecipeRatingsCache(int recipeId);
}
