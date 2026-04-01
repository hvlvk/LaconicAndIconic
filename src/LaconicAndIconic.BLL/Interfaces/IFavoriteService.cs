using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IFavoriteService
{
    Task<Result> AddFavoriteAsync(int userId, int recipeId);
    Task<Result> RemoveFavoriteAsync(int userId, int recipeId);
    Task<Result<IEnumerable<RecipeDto>>> GetFavoritesByUserAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int recipeId);
}
