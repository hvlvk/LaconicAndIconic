using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IRecipeService
{
    Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto);
    Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId);
    Task<Result> DeleteRecipeAsync(int recipeId, int authorId);
}
