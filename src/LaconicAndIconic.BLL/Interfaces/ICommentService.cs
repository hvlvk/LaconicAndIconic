using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface ICommentService
{
    Task<Result> CreateAsync(CreateCommentDto dto, int userId);
    Task<Result<IEnumerable<CommentDto>>> GetCommentsByRecipeIdAsync(int recipeId, int? currentUserId = null);
    Task<Result> ToggleLikeAsync(int commentId, int userId);
    Task<Result> DeleteAsync(int commentId, int userId);
    Task<Result> EditAsync(EditCommentDto dto, int userId);
    Task<Result<CommentDto>> GetCommentByIdAsync(int commentId);
}
