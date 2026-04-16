using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.BLL.Services;

public class CommentService : ICommentService
{
    private readonly IRepository<Comment> _commentRepository;

    public CommentService(IRepository<Comment> commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result> CreateAsync(CreateCommentDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return Result.Failure("Коментар не може бути порожнім");
        }

        var comment = new Comment
        {
            RecipeId = dto.RecipeId,
            Content = dto.Content,
            AuthorId = userId,
            Recipe = null!,
            Author = null!
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<CommentDto>>> GetCommentsByRecipeIdAsync(int recipeId)
    {
        var comments = await _commentRepository.GetQueryable()
            .Where(c => c.RecipeId == recipeId)
            .Include(c => c.Author)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                RecipeId = c.RecipeId,
                AuthorId = c.AuthorId,
                AuthorName = c.Author.UserName ?? string.Empty,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Result<IEnumerable<CommentDto>>.Success(comments);
    }

    public async Task<Result> DeleteAsync(int commentId, int userId)
    {
        var comment = await _commentRepository.GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return Result.Failure("Коментар не знайдено");
        }

        if (comment.AuthorId != userId)
        {
            return Result.Failure("Ви не можете видалити чужий коментар");
        }

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync();

        return Result.Success();
    }
}
