using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;

namespace LaconicAndIconic.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IFileService _fileService;
    private readonly IRepository<Category> _categoryRepository;

    public RecipeService(IRecipeRepository recipeRepository, IFileService fileService, IRepository<Category> categoryRepository)
    {
        _recipeRepository = recipeRepository;
        _fileService = fileService;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result<RecipeDto>.Failure("Назва обов'язкова");
        }

        var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
        if (!categoryExists)
        {
            return Result<RecipeDto>.Failure("Категорія не знайдена");
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
            return Result<RecipeDto>.Failure("Рецепт не знайдено");
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
            return Result.Failure("Рецепт не знайдено");
        }

        if (recipe.AuthorId != authorId)
        {
            return Result.Failure("Ви можете видаляти тільки свої рецепти");
        }

        _recipeRepository.Remove(recipe);
        await _recipeRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<RecipeSearchResultDto>> SearchRecipesAsync(RecipeSearchFilterDto filter)
    {
        var dalFilter = new RecipeSearchFilter
        {
            SearchTerm = filter.SearchTerm,
            CategoryId = filter.CategoryId,
            SortBy = filter.SortBy,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        var dalResult = await _recipeRepository.SearchAsync(dalFilter);

        var dtos = dalResult.Recipes.Select(MapToDto).ToList();

        var result = new RecipeSearchResultDto
        {
            Recipes = dtos,
            TotalCount = dalResult.TotalCount,
            PageNumber = dalResult.PageNumber,
            PageSize = dalResult.PageSize,
            SearchTerm = dalResult.SearchTerm,
            CategoryId = dalResult.CategoryId,
            SortBy = dalResult.SortBy
        };

        return Result<RecipeSearchResultDto>.Success(result);
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
            AuthorName = recipe.Author.UserName ?? string.Empty,
            AuthorProfilePicturePath = recipe.Author?.ProfilePicturePath
        };
    }
}
