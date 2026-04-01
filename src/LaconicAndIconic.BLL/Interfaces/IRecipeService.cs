using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IRecipeService
{
    Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto);
    Task<Result<RecipeDto>> GetRecipeByIdAsync(int recipeId);
    Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId);
    Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync();
    Task<Result> DeleteRecipeAsync(int recipeId, int authorId);
    Task<Result<RecipeDto>> GetRecipeByIdAsync(int recipeId);
}
