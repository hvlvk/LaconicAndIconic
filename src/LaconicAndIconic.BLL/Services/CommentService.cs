using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;

namespace LaconicAndIconic.BLL.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IRepository<CommentLike> _commentLikeRepository;

    public CommentService(ICommentRepository commentRepository, IRepository<CommentLike> commentLikeRepository)
    {
        _commentRepository = commentRepository;
        _commentLikeRepository = commentLikeRepository;
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

    public async Task<Result<IEnumerable<CommentDto>>> GetCommentsByRecipeIdAsync(int recipeId, int? currentUserId = null)
    {
        var commentsList = await _commentRepository.GetCommentsByRecipeIdAsync(recipeId);
        
        var comments = commentsList.Select(c => new CommentDto
        {
            Id = c.Id,
            RecipeId = c.RecipeId,
            AuthorId = c.AuthorId,
            AuthorName = c.Author?.UserName ?? string.Empty,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            LikesCount = c.Likes.Count,
            IsLikedByCurrentUser = currentUserId.HasValue && c.Likes.Any(l => l.UserId == currentUserId.Value)
        }).ToList();

        return comments;
    }

    public async Task<Result> ToggleLikeAsync(int commentId, int userId)
    {
        var commentExists = await _commentRepository.ExistsAsync(commentId);
        if (!commentExists)
        {
            return Result.Failure("Коментар не знайдено");
        }

        var existingLike = await _commentLikeRepository.FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existingLike == null)
        {
            await _commentLikeRepository.AddAsync(new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                Comment = null!,
                User = null!
            });
        }
        else
        {
            _commentLikeRepository.Remove(existingLike);
        }

        await _commentLikeRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int commentId, int userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);

        if (comment == null)
        {
            return Result.Failure("Коментар не знадено");
        }

        

        
        if (userId != 0)
        {
            if (comment.AuthorId != userId)
            {
                return Result.Failure("Ви не можете видалити чужий коментар");
            }
        }

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> EditAsync(EditCommentDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return Result.Failure("Коментар не може бути порожнім");
        }

        var comment = await _commentRepository.GetByIdAsync(dto.Id);

        if (comment == null)
        {
            return Result.Failure("Коментар не знайдено");
        }

        if (comment.AuthorId != userId)
        {
            return Result.Failure("Ви не можете редагувати чужий коментар");
        }

        comment.Content = dto.Content;
        await _commentRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<CommentDto>> GetCommentByIdAsync(int commentId)
    {
        var comment = await _commentRepository.GetWithDetailsAsync(commentId);

        if (comment == null)
        {
            return "Коментар не знайдено";
        }

        var dto = new CommentDto
        {
            Id = comment.Id,
            RecipeId = comment.RecipeId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Author.UserName ?? string.Empty,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            LikesCount = comment.Likes.Count,
            IsLikedByCurrentUser = false
        };

        return dto;
    }
}
