using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;

namespace LaconicAndIconic.BLL.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<Recipe> _recipeRepository;

    public FavoriteService(IRepository<Favorite> favoriteRepository, IRepository<Recipe> recipeRepository)
    {
        _favoriteRepository = favoriteRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<Result> AddFavoriteAsync(int userId, int recipeId)
    {
        var existing = await _favoriteRepository.FindAsync(
            f => f.UserId == userId && f.RecipeId == recipeId);

        if (existing.Any())
        {
            return Result.Success();
        }

        var recipeExists = await _recipeRepository.ExistsAsync(recipeId);
        if (!recipeExists)
        {
            return Result.Failure("Recipe not found");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            RecipeId = recipeId
        };

        await _favoriteRepository.AddAsync(favorite);
        await _favoriteRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> RemoveFavoriteAsync(int userId, int recipeId)
    {
        var existing = await _favoriteRepository.FindAsync(
            f => f.UserId == userId && f.RecipeId == recipeId);

        var favorite = existing.FirstOrDefault();
        if (favorite is null)
        {
            return Result.Failure("Favorite not found");
        }

        _favoriteRepository.Remove(favorite);
        await _favoriteRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetFavoritesByUserAsync(int userId)
    {
        var favorites = await _favoriteRepository.FindAsync(
            f => f.UserId == userId,
            f => f.Recipe.Category,
            f => f.Recipe.Author);

        var dtos = favorites
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new RecipeDto
            {
                Id = f.Recipe.Id,
                Title = f.Recipe.Title,
                Description = f.Recipe.Description,
                ImagePath = f.Recipe.ImagePath,
                PrepTimeMin = f.Recipe.PrepTimeMin,
                CategoryId = f.Recipe.CategoryId,
                CategoryName = f.Recipe.Category?.Name ?? string.Empty,
                AuthorId = f.Recipe.AuthorId,
                AuthorName = f.Recipe.Author?.UserName ?? string.Empty
            });

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<bool> IsFavoriteAsync(int userId, int recipeId)
    {
        var result = await _favoriteRepository.FindAsync(
            f => f.UserId == userId && f.RecipeId == recipeId);

        return result.Any();
    }
}
