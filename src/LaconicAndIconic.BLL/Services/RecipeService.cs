using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.BLL.Services;

public class RecipeService : IRecipeService
{
    private const string AllRecipesCacheKeyPrefix = "all_recipes";
    private const string RecipeCacheKeyPrefix = "recipe_";
    private const string AuthorRecipesCacheKeyPrefix = "author_recipes_";

    private readonly IRecipeRepository _recipeRepository;
    private readonly IFileService _fileService;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Rating> _ratingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly TimeSpan _recipesCacheDuration;

    public RecipeService(
        IRecipeRepository recipeRepository,
        IFileService fileService,
        IRepository<Category> categoryRepository,
        IRepository<Rating> ratingRepository,
        IUserRepository userRepository,
        IMemoryCache memoryCache,
        ICacheInvalidationService cacheInvalidationService,
        IOptions<CachingOptions> cachingOptions)
    {
        _recipeRepository = recipeRepository;
        _fileService = fileService;
        _categoryRepository = categoryRepository;
        _ratingRepository = ratingRepository;
        _userRepository = userRepository;
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _cacheInvalidationService = cacheInvalidationService ?? throw new ArgumentNullException(nameof(cacheInvalidationService));
        ArgumentNullException.ThrowIfNull(cachingOptions);
        _recipesCacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.RecipesCacheLifetimeMinutes);
    }

    public async Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return "Назва обов'язкова";
        }

        if (dto.Servings <= 0)
        {
            return "Кількість порцій має бути більше 0";
        }

        if (string.IsNullOrWhiteSpace(dto.Ingredients))
        {
            return "Інгредієнти обов'язкові";
        }

        if (string.IsNullOrWhiteSpace(dto.CookingMethod))
        {
            return "Спосіб приготування обов'язковий";
        }

        var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
        if (!categoryExists)
        {
            return "Категорія не знайдена";
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
            Servings = dto.Servings,
            Ingredients = dto.Ingredients,
            CookingMethod = dto.CookingMethod,
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            ImagePath = imagePath
        };

        await _recipeRepository.AddAsync(recipe);
        await _recipeRepository.SaveChangesAsync();

        // Invalidate relevant caches
        _cacheInvalidationService.InvalidateRecipesCache();
        _cacheInvalidationService.InvalidateAuthorRecipesCache(authorId);

        var responseDto = new RecipeDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            ImagePath = recipe.ImagePath,
            PrepTimeMin = recipe.PrepTimeMin,
            Servings = recipe.Servings,
            Ingredients = recipe.Ingredients,
            CookingMethod = recipe.CookingMethod,
            CategoryId = recipe.CategoryId,
            AuthorId = recipe.AuthorId
        };

        return responseDto;
    }

    public async Task<Result> UpdateRecipeAsync(int recipeId, int authorId, UpdateRecipeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result.Failure("Назва обов'язкова");
        }

        if (dto.Servings <= 0)
        {
            return Result.Failure("Кількість порцій має бути більше 0");
        }

        if (string.IsNullOrWhiteSpace(dto.Ingredients))
        {
            return Result.Failure("Інгредієнти обов'язкові");
        }

        if (string.IsNullOrWhiteSpace(dto.CookingMethod))
        {
            return Result.Failure("Спосіб приготування обов'язковий");
        }

        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
        {
            return Result.Failure("Рецепт не знайдено");
        }

        if (!string.IsNullOrWhiteSpace(recipe.ExternalSource))
        {
            return Result.Failure("Імпортовані рецепти не можна редагувати");
        }

        if (recipe.AuthorId != authorId)
        {
            return Result.Failure("Ви можете редагувати тільки свої рецепти");
        }

        var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
        if (!categoryExists)
        {
            return Result.Failure("Категорія не знайдена");
        }

        if (dto.Image != null && dto.Image.Length > 0)
        {
            recipe.ImagePath = await _fileService.SaveFileAsync(dto.Image, "recipes");
        }

        recipe.Title = dto.Title;
        recipe.Description = dto.Description;
        recipe.PrepTimeMin = dto.PrepTimeMin;
        recipe.Servings = dto.Servings;
        recipe.Ingredients = dto.Ingredients;
        recipe.CookingMethod = dto.CookingMethod;
        recipe.CategoryId = dto.CategoryId;

        _recipeRepository.Update(recipe);
        await _recipeRepository.SaveChangesAsync();

        // Invalidate relevant caches
        _cacheInvalidationService.InvalidateRecipesCache();
        _cacheInvalidationService.InvalidateRecipeCache(recipeId);
        _cacheInvalidationService.InvalidateAuthorRecipesCache(authorId);

        return Result.Success();
    }

    public async Task<Result<RecipeDto>> GetRecipeByIdAsync(int recipeId, int? currentUserId = null)
    {
        var cacheKey = $"{RecipeCacheKeyPrefix}{recipeId}";
        if (currentUserId == null && _memoryCache.TryGetValue(cacheKey, out RecipeDto? cachedRecipe))
        {
            return cachedRecipe!;
        }

        var recipes = await _recipeRepository.FindAsync(r => r.Id == recipeId, r => r.Category, r => r.Author, r => r.Ratings);
        var recipe = recipes.FirstOrDefault();

        if (recipe == null)
        {
            return "Рецепт не знайдено";
        }

        var dto = MapToDto(recipe, currentUserId);

        if (currentUserId == null)
        {
            _memoryCache.Set(cacheKey, dto, _recipesCacheDuration);
        }

        return dto;
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId)
    {
        var cacheKey = $"{AuthorRecipesCacheKeyPrefix}{authorId}";

        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<RecipeDto>? cachedRecipes))
        {
            return Result<IEnumerable<RecipeDto>>.Success(cachedRecipes!);
        }

        var recipes = await _recipeRepository
            .FindAsync(
                r => r.AuthorId == authorId,
                r => r.Category,
                r => r.Author,
                r => r.Ratings);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(recipe => MapToDto(recipe))
            .ToList();

        _memoryCache.Set(cacheKey, dtos, _recipesCacheDuration);

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync()
    {
        const string cacheKey = AllRecipesCacheKeyPrefix;

        // Try to get from cache
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<RecipeDto>? cachedRecipes))
        {
            return Result<IEnumerable<RecipeDto>>.Success(cachedRecipes!);
        }

        var recipes = await _recipeRepository
            .FindAsync(_ => true, r => r.Category, r => r.Author, r => r.Ratings);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(recipe => MapToDto(recipe))
            .ToList();

        _memoryCache.Set(cacheKey, dtos, _recipesCacheDuration);

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<Result> DeleteRecipeAsync(int recipeId, int authorId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
        {
            return Result.Failure("Рецепт не знайдено");
        }

        if (!string.IsNullOrWhiteSpace(recipe.ExternalSource))
        {
            return Result.Failure("Імпортовані рецепти не можна видаляти");
        }

        if (recipe.AuthorId != authorId)
        {
            return Result.Failure("Ви можете видаляти тільки свої рецепти");
        }

        _recipeRepository.Remove(recipe);
        await _recipeRepository.SaveChangesAsync();

        _cacheInvalidationService.InvalidateRecipesCache();
        _cacheInvalidationService.InvalidateRecipeCache(recipeId);
        _cacheInvalidationService.InvalidateAuthorRecipesCache(authorId);

        return Result.Success();
    }

    public async Task<Result> RateRecipeAsync(int recipeId, int userId, int score)
    {
        if (score < 1 || score > 5)
        {
            return Result.Failure("Оцінка має бути від 1 до 5");
        }

        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
        {
            return Result.Failure("Рецепт не знайдено");
        }

        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Користувача не знайдено");
        }

        var ratings = await _ratingRepository.FindAsync(r => r.RecipeId == recipeId && r.UserId == userId);
        var rating = ratings.FirstOrDefault();

        if (rating == null)
        {
            rating = new Rating
            {
                RecipeId = recipeId,
                UserId = userId,
                Score = score,
                Recipe = recipe,
                User = user
            };

            await _ratingRepository.AddAsync(rating);
        }
        else
        {
            rating.Score = score;
            _ratingRepository.Update(rating);
        }

        await _ratingRepository.SaveChangesAsync();

        // Invalidate relevant caches when a rating is added or updated
        _cacheInvalidationService.InvalidateRecipeCache(recipeId);
        _cacheInvalidationService.InvalidateRecipeRatingsCache(recipeId);

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

        var dtos = dalResult.Recipes.Select(recipe => MapToDto(recipe)).ToList();

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

        return result;
    }

    private static RecipeDto MapToDto(Recipe recipe, int? currentUserId = null)
    {
        var ratings = recipe.Ratings.ToList();
        var ratingCount = ratings.Count;
        var averageRating = ratingCount == 0 ? 0 : ratings.Average(r => r.Score);
        int? currentUserRating = currentUserId.HasValue
            ? ratings.FirstOrDefault(r => r.UserId == currentUserId.Value)?.Score
            : null;

        return new RecipeDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            ImagePath = recipe.ImagePath,
            PrepTimeMin = recipe.PrepTimeMin,
            Servings = recipe.Servings,
            Ingredients = recipe.Ingredients,
            CookingMethod = recipe.CookingMethod,
            AverageRating = averageRating,
            RatingCount = ratingCount,
            CurrentUserRating = currentUserRating,
            CategoryId = recipe.CategoryId,
            CategoryName = recipe.Category.Name,
            AuthorId = recipe.AuthorId,
            AuthorName = recipe.Author.UserName ?? string.Empty,
            AuthorProfilePicturePath = recipe.Author.ProfilePicturePath,
            IsExternal = !string.IsNullOrWhiteSpace(recipe.ExternalSource),
            ExternalSource = recipe.ExternalSource,
            ExternalId = recipe.ExternalId
        };
    }
}
