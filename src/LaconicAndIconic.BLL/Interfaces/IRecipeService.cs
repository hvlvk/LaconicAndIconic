using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IRecipeService
{
    Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto);
    Task<Result> UpdateRecipeAsync(int recipeId, int authorId, UpdateRecipeDto dto);
    Task<Result<RecipeDto>> GetRecipeByIdAsync(int recipeId, int? currentUserId = null);
    Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId);
    Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync();
    Task<Result> DeleteRecipeAsync(int recipeId, int authorId);
    Task<Result> RateRecipeAsync(int recipeId, int userId, int score);
    Task<Result<RecipeSearchResultDto>> SearchRecipesAsync(RecipeSearchFilterDto filter, int? currentUserId = null);
    Task<Result> SaveRecipeAsync(int recipeId, int userId);
    Task<Result> UnsaveRecipeAsync(int recipeId, int userId);
    Task<Result<IEnumerable<RecipeDto>>> GetSavedRecipesByUserIdAsync(int userId);
    Task<Result<bool>> IsRecipeSavedAsync(int recipeId, int userId);
}
