using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.BLL.Decorators
{
    public class CachedRecipeService : IRecipeService
    {
        private const string AllRecipesCacheKeyPrefix = "all_recipes";
        private const string RecipeCacheKeyPrefix = "recipe_";
        private const string AuthorRecipesCacheKeyPrefix = "author_recipes_";

        private readonly IRecipeService _innerService;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheInvalidationService _cacheInvalidationService;
        private readonly TimeSpan _recipesCacheDuration;
        public CachedRecipeService(
            IRecipeService innerService,
            IMemoryCache memoryCache,
            ICacheInvalidationService cacheInvalidationService,
            IOptions<CachingOptions> cachingOptions)
        {
            _innerService = innerService;
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _cacheInvalidationService = cacheInvalidationService ?? throw new ArgumentNullException(nameof(cacheInvalidationService));

            ArgumentNullException.ThrowIfNull(cachingOptions);
            _recipesCacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.RecipesCacheLifetimeMinutes);
        }

        public async Task<Result<RecipeDto>> GetRecipeByIdAsync(int recipeId, int? currentUserId = null)
        {
            var cacheKey = $"{RecipeCacheKeyPrefix}{recipeId}";

            if (currentUserId == null && _memoryCache.TryGetValue(cacheKey, out RecipeDto? cachedRecipe))
            {
                return cachedRecipe!;
            }

            var result = await _innerService.GetRecipeByIdAsync(recipeId, currentUserId);

            if (result.IsSuccess && currentUserId == null)
            {
                _memoryCache.Set(cacheKey, result.Value, _recipesCacheDuration);
            }

            return result;
        }

        public async Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId)
        {
            var cacheKey = $"{AuthorRecipesCacheKeyPrefix}{authorId}";

            if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<RecipeDto>? cachedRecipes))
            {
                return Result<IEnumerable<RecipeDto>>.Success(cachedRecipes!);
            }

            var result = await _innerService.GetRecipesByAuthorIdAsync(authorId);

            if (result.IsSuccess)
            {
                _memoryCache.Set(cacheKey, result.Value, _recipesCacheDuration);
            }

            return result;
        }

        public async Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync()
        {
            const string cacheKey = AllRecipesCacheKeyPrefix;

            if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<RecipeDto>? cachedRecipes))
            {
                return Result<IEnumerable<RecipeDto>>.Success(cachedRecipes!);
            }

            var result = await _innerService.GetAllRecipesAsync();

            if (result.IsSuccess)
            {
                _memoryCache.Set(cacheKey, result.Value, _recipesCacheDuration);
            }

            return result;
        }

        public async Task<Result<RecipeSearchResultDto>> SearchRecipesAsync(RecipeSearchFilterDto filter)
        {
            return await _innerService.SearchRecipesAsync(filter);
        }

        public async Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto)
        {
            var result = await _innerService.CreateRecipeAsync(authorId, dto);

            if (result.IsSuccess)
            {
                _cacheInvalidationService.InvalidateRecipesCache();
                _cacheInvalidationService.InvalidateAuthorRecipesCache(authorId);
            }

            return result;
        }

        public async Task<Result> UpdateRecipeAsync(int recipeId, int authorId, UpdateRecipeDto dto)
        {
            var result = await _innerService.UpdateRecipeAsync(recipeId, authorId, dto);

            if (result.IsSuccess)
            {
                _cacheInvalidationService.InvalidateRecipesCache();
                _cacheInvalidationService.InvalidateRecipeCache(recipeId);
                _cacheInvalidationService.InvalidateAuthorRecipesCache(authorId);
            }

            return result;
        }

        public async Task<Result> DeleteRecipeAsync(int recipeId, int authorId)
        {
            var result = await _innerService.DeleteRecipeAsync(recipeId, authorId);

            if (result.IsSuccess)
            {
                _cacheInvalidationService.InvalidateRecipesCache();
                _cacheInvalidationService.InvalidateRecipeCache(recipeId);
                _cacheInvalidationService.InvalidateAuthorRecipesCache(authorId);
            }

            return result;
        }

        public async Task<Result> RateRecipeAsync(int recipeId, int userId, int score)
        {
            var result = await _innerService.RateRecipeAsync(recipeId, userId, score);

            if (result.IsSuccess)
            {
                _cacheInvalidationService.InvalidateRecipeCache(recipeId);
                _cacheInvalidationService.InvalidateRecipeRatingsCache(recipeId);
            }

            return result;
        }
    }
}
