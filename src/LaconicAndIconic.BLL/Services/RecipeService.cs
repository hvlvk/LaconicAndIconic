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
    private const string SavedRecipesCacheKeyPrefix = "saved_recipes_";

    private readonly IRecipeRepository _recipeRepository;
    private readonly IFileService _fileService;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Rating> _ratingRepository;
    private readonly IRepository<SavedRecipe> _savedRecipeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly TimeSpan _recipesCacheDuration;

    public RecipeService(
        IRecipeRepository recipeRepository,
        IFileService fileService,
        IRepository<Category> categoryRepository,
        IRepository<Rating> ratingRepository,
        IRepository<SavedRecipe> savedRecipeRepository,
        IUserRepository userRepository,
        IMemoryCache memoryCache,
        ICacheInvalidationService cacheInvalidationService,
        IOptions<CachingOptions> cachingOptions)
    {
        _recipeRepository = recipeRepository;
        _fileService = fileService;
        _categoryRepository = categoryRepository;
        _ratingRepository = ratingRepository;
        _savedRecipeRepository = savedRecipeRepository;
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
            return Result<RecipeDto>.Failure("Назва обов'язкова");
        }

        if (dto.Servings <= 0)
        {
            return Result<RecipeDto>.Failure("Кількість порцій має бути більше 0");
        }

        if (string.IsNullOrWhiteSpace(dto.Ingredients))
        {
            return Result<RecipeDto>.Failure("Інгредієнти обов'язкові");
        }

        if (string.IsNullOrWhiteSpace(dto.CookingMethod))
        {
            return Result<RecipeDto>.Failure("Спосіб приготування обов'язковий");
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
            Servings = dto.Servings,
            Ingredients = dto.Ingredients,
            CookingMethod = dto.CookingMethod,
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            ImagePath = imagePath
        };

        await _recipeRepository.AddAsync(recipe);
        await _recipeRepository.SaveChangesAsync();

        // Automatically save the recipe for the author
        var savedRecipe = new SavedRecipe
        {
            RecipeId = recipe.Id,
            UserId = authorId
        };
        await _savedRecipeRepository.AddAsync(savedRecipe);
        await _savedRecipeRepository.SaveChangesAsync();

        // Invalidate relevant caches
        _cacheInvalidationService.InvalidateRecipesCache();
        _cacheInvalidationService.InvalidateAuthorRecipesCache(authorId);
        _cacheInvalidationService.InvalidateSavedRecipesCache(authorId);

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

        return Result<RecipeDto>.Success(responseDto);
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
        // Try to get from cache (but only if no current user is involved)
        var cacheKey = $"{RecipeCacheKeyPrefix}{recipeId}";
        if (currentUserId == null && _memoryCache.TryGetValue(cacheKey, out RecipeDto? cachedRecipe))
        {
            return Result<RecipeDto>.Success(cachedRecipe!);
        }

        var recipes = await _recipeRepository.FindAsync(r => r.Id == recipeId, r => r.Category, r => r.Author, r => r.Ratings);
        var recipe = recipes.FirstOrDefault();

        if (recipe == null)
        {
            return Result<RecipeDto>.Failure("Рецепт не знайдено");
        }

        bool isSaved = false;
        if (currentUserId.HasValue)
        {
            var savedRecipes = await _savedRecipeRepository.FindAsync(sr => sr.RecipeId == recipeId && sr.UserId == currentUserId.Value);
            isSaved = savedRecipes.Any();
        }

        var dto = MapToDto(recipe, currentUserId, isSaved);

        // Cache only if no current user is involved (user-specific data shouldn't be cached)
        if (currentUserId == null)
        {
            _memoryCache.Set(cacheKey, dto, _recipesCacheDuration);
        }

        return Result<RecipeDto>.Success(dto);
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId)
    {
        var cacheKey = $"{AuthorRecipesCacheKeyPrefix}{authorId}";

        // Try to get from cache
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<RecipeDto>? cachedRecipes))
        {
            return Result<IEnumerable<RecipeDto>>.Success(cachedRecipes!);
        }

        var recipes = await _recipeRepository
            .FindAsync(r => r.AuthorId == authorId, r => r.Category, r => r.Author, r => r.Ratings);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(recipe => MapToDto(recipe))
            .ToList();

        // Cache the result
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

        // Cache the result
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

        if (recipe.AuthorId != authorId)
        {
            return Result.Failure("Ви можете видаляти тільки свої рецепти");
        }

        _recipeRepository.Remove(recipe);
        await _recipeRepository.SaveChangesAsync();

        // Invalidate relevant caches
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

    public async Task<Result<RecipeSearchResultDto>> SearchRecipesAsync(RecipeSearchFilterDto filter, int? currentUserId = null)
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

        var dtos = dalResult.Recipes.Select(recipe =>
        {
            bool isSaved = false;
            if (currentUserId.HasValue && recipe.SavedRecipes != null)
            {
                isSaved = recipe.SavedRecipes.Any(sr => sr.UserId == currentUserId.Value);
            }
            return MapToDto(recipe, currentUserId, isSaved);
        }).ToList();

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

    private static RecipeDto MapToDto(Recipe recipe, int? currentUserId = null, bool isSaved = false)
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
            IsSaved = isSaved,
            CategoryId = recipe.CategoryId,
            CategoryName = recipe.Category.Name,
            AuthorId = recipe.AuthorId,
            AuthorName = recipe.Author.UserName ?? string.Empty,
            AuthorProfilePicturePath = recipe.Author.ProfilePicturePath
        };
    }

    public async Task<Result> SaveRecipeAsync(int recipeId, int userId)
    {
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

        var savedRecipes = await _savedRecipeRepository.FindAsync(sr => sr.RecipeId == recipeId && sr.UserId == userId);
        if (savedRecipes.Any())
        {
            return Result.Failure("Рецепт вже збережено");
        }

        var savedRecipe = new SavedRecipe
        {
            RecipeId = recipeId,
            UserId = userId
        };

        await _savedRecipeRepository.AddAsync(savedRecipe);
        await _savedRecipeRepository.SaveChangesAsync();

        // Invalidate saved recipes cache for this user
        _cacheInvalidationService.InvalidateSavedRecipesCache(userId);

        return Result.Success();
    }

    public async Task<Result> UnsaveRecipeAsync(int recipeId, int userId)
    {
        var savedRecipes = await _savedRecipeRepository.FindAsync(sr => sr.RecipeId == recipeId && sr.UserId == userId);
        var savedRecipe = savedRecipes.FirstOrDefault();

        if (savedRecipe == null)
        {
            return Result.Failure("Рецепт не збережено");
        }

        _savedRecipeRepository.Remove(savedRecipe);
        await _savedRecipeRepository.SaveChangesAsync();

        // Invalidate saved recipes cache for this user
        _cacheInvalidationService.InvalidateSavedRecipesCache(userId);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetSavedRecipesByUserIdAsync(int userId)
    {
        var cacheKey = $"{SavedRecipesCacheKeyPrefix}{userId}";

        // Try to get from cache
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<RecipeDto>? cachedRecipes))
        {
            return Result<IEnumerable<RecipeDto>>.Success(cachedRecipes!);
        }

        var savedRecipes = await _savedRecipeRepository.FindAsync(
            sr => sr.UserId == userId,
            sr => sr.Recipe,
            sr => sr.Recipe.Category,
            sr => sr.Recipe.Author,
            sr => sr.Recipe.Ratings);

        var dtos = savedRecipes
            .OrderByDescending(sr => sr.CreatedAt)
            .Select(sr => MapToDto(sr.Recipe))
            .ToList();

        // Cache the result
        _memoryCache.Set(cacheKey, dtos, _recipesCacheDuration);

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<Result<bool>> IsRecipeSavedAsync(int recipeId, int userId)
    {
        var savedRecipes = await _savedRecipeRepository.FindAsync(sr => sr.RecipeId == recipeId && sr.UserId == userId);
        return Result<bool>.Success(savedRecipes.Any());
    }
}
