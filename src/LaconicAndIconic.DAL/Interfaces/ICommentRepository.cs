using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetCommentsByRecipeIdAsync(int recipeId);
    Task<Comment?> GetWithDetailsAsync(int commentId);
}
