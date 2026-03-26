using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;

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

    public async Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId)
    {
        var recipes = await _recipeRepository
            .FindAsync(r => r.AuthorId == authorId, r => r.Category, r => r.Author);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RecipeDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                ImagePath = r.ImagePath,
                PrepTimeMin = r.PrepTimeMin,
                CategoryId = r.CategoryId,
                CategoryName = r.Category?.Name ?? string.Empty,
                AuthorId = r.AuthorId,
                AuthorName = r.Author?.UserName ?? string.Empty
            });

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }
}
