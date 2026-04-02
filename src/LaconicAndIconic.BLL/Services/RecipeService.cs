using System.Linq.Expressions;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IRepository<Recipe> _recipeRepository;
    private readonly IFileService _fileService;
    private readonly IRepository<Category> _categoryRepository;

    public RecipeService(IRepository<Recipe> recipeRepository, IFileService fileService, IRepository<Category> categoryRepository)
    {
        _recipeRepository = recipeRepository;
        _fileService = fileService;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result<RecipeDto>.Failure("Title is required");
        }

        var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
        if (!categoryExists)
        {
            return Result<RecipeDto>.Failure("Category not found");
        }

        string? imagePath = null;
        if (dto.Image != null && dto.Image.Length > 0)
        {
            imagePath = await _fileService.SaveFileAsync(dto.Image, "recipes");
        }

        var recipe = new Recipe
        {
            Title = dto.Title,
            Description = dto.Description,
            PrepTimeMin = dto.PrepTimeMin,
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            ImagePath = imagePath
        };

        await _recipeRepository.AddAsync(recipe);
        await _recipeRepository.SaveChangesAsync();

        var responseDto = new RecipeDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            ImagePath = recipe.ImagePath,
            PrepTimeMin = recipe.PrepTimeMin,
            CategoryId = recipe.CategoryId,
            AuthorId = recipe.AuthorId
        };

        return Result<RecipeDto>.Success(responseDto);
    }

    public async Task<Result<RecipeDto>> GetRecipeByIdAsync(int recipeId)
    {
        var recipes = await _recipeRepository.FindAsync(r => r.Id == recipeId, r => r.Category, r => r.Author);
        var recipe = recipes.FirstOrDefault();

        if (recipe == null)
        {
            return Result<RecipeDto>.Failure("Recipe not found");
        }

        return Result<RecipeDto>.Success(MapToDto(recipe));
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId)
    {
        var recipes = await _recipeRepository
            .FindAsync(r => r.AuthorId == authorId, r => r.Category, r => r.Author);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapToDto);

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync()
    {
        var recipes = await _recipeRepository
            .FindAsync(_ => true, r => r.Category, r => r.Author);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapToDto);

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<Result> DeleteRecipeAsync(int recipeId, int authorId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
        {
            return Result.Failure("Recipe not found");
        }

        if (recipe.AuthorId != authorId)
        {
            return Result.Failure("You can only delete your own recipes");
        }

        _recipeRepository.Remove(recipe);
        await _recipeRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<RecipeSearchResultDto>> SearchRecipesAsync(RecipeSearchFilterDto filter)
    {
        var dbQuery = _recipeRepository.GetQueryable().AsNoTracking();

        if (filter.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.CategoryId == filter.CategoryId.Value);
        }

        var candidates = await dbQuery.Select(r => new RecipeSearchCandidate
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            CategoryName = r.Category.Name,
            CreatedAt = r.CreatedAt,
            PrepTimeMin = r.PrepTimeMin
        }).ToListAsync();

        IEnumerable<RecipeSearchCandidate> filteredResults = candidates;
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchWords = filter.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            filteredResults = filteredResults.Where(c =>
                searchWords.All(word =>
                    c.Title.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    c.CategoryName.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(word, StringComparison.OrdinalIgnoreCase))));
        }

        var sortedResults = ApplySorting(filteredResults, filter.SortBy).ToList();

        var totalCount = sortedResults.Count;
        var pagedIds = sortedResults
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => c.Id)
            .ToList();

        var recipes = await _recipeRepository.GetQueryable()
            .Include(r => r.Category)
            .Include(r => r.Author)
            .Where(r => pagedIds.Contains(r.Id))
            .AsNoTracking()
            .ToListAsync();

        var finalRecipes = recipes
            .OrderBy(r => pagedIds.IndexOf(r.Id))
            .Select(MapToDto)
            .ToList();

        var result = new RecipeSearchResultDto
        {
            Recipes = finalRecipes,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            SearchTerm = filter.SearchTerm,
            CategoryId = filter.CategoryId,
            SortBy = filter.SortBy
        };

        return Result<RecipeSearchResultDto>.Success(result);
    }

    private sealed class RecipeSearchCandidate
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PrepTimeMin { get; set; }
    }

    private static IEnumerable<RecipeSearchCandidate> ApplySorting(IEnumerable<RecipeSearchCandidate> query, string? sortBy)
    {
        return sortBy switch
        {
            "title_asc" => query.OrderBy(r => r.Title),
            "title_desc" => query.OrderByDescending(r => r.Title),
            "prepTime_asc" => query.OrderBy(r => r.PrepTimeMin),
            "prepTime_desc" => query.OrderByDescending(r => r.PrepTimeMin),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };
    }

    private static RecipeDto MapToDto(Recipe recipe)
    {
        return new RecipeDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            ImagePath = recipe.ImagePath,
            PrepTimeMin = recipe.PrepTimeMin,
            CategoryId = recipe.CategoryId,
            CategoryName = recipe.Category.Name,
            AuthorId = recipe.AuthorId,
            AuthorName = recipe.Author.UserName ?? string.Empty
        };
    }
}
