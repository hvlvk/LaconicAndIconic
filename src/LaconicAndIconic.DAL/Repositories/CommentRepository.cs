using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetCommentsByRecipeIdAsync(int recipeId)
    {
        return await Context.Set<Comment>()
            .Where(c => c.RecipeId == recipeId)
            .Include(c => c.Author)
            .OrderByDescending(c => c.Likes.Count)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetWithDetailsAsync(int commentId)
    {
        return await Context.Set<Comment>()
            .Include(c => c.Author)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }
}
