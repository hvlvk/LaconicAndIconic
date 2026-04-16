using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface ICommentService
{
    Task<Result> CreateAsync(CreateCommentDto dto, int userId);
    Task<Result<IEnumerable<CommentDto>>> GetCommentsByRecipeIdAsync(int recipeId);
}
