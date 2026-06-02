using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.BLL.Decorators;

public class CachedCategoryService : ICategoryService
{
    private const string AllCategoriesCacheKey = "all_categories";

    private readonly ICategoryService _innerService;
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly TimeSpan _cacheDuration;

    public CachedCategoryService(
        ICategoryService innerService,
        IMemoryCache memoryCache,
        ICacheInvalidationService cacheInvalidationService,
        IOptions<CachingOptions> cachingOptions)
    {
        _innerService = innerService;
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _cacheInvalidationService = cacheInvalidationService ?? throw new ArgumentNullException(nameof(cacheInvalidationService));

        ArgumentNullException.ThrowIfNull(cachingOptions);
        _cacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.CategoriesCacheLifetimeMinutes);
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        if (_memoryCache.TryGetValue(AllCategoriesCacheKey, out IEnumerable<CategoryDto>? cachedCategories))
        {
            return Result<IEnumerable<CategoryDto>>.Success(cachedCategories!);
        }

        var result = await _innerService.GetAllAsync();

        if (result.IsSuccess)
        {
            _memoryCache.Set(AllCategoriesCacheKey, result.Value, _cacheDuration);
        }

        return result;
    }
    public async Task<Result> CreateAsync(string name)
    {
        var result = await _innerService.CreateAsync(name);

        if (result.IsSuccess)
        {
            _cacheInvalidationService.InvalidateCategoriesCache();
        }

        return result;
    }

    public async Task<Result> UpdateAsync(int id, string name)
    {
        var result = await _innerService.UpdateAsync(id, name);

        if (result.IsSuccess)
        {
            _cacheInvalidationService.InvalidateCategoriesCache();
        }

        return result;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var result = await _innerService.DeleteAsync(id);

        if (result.IsSuccess)
        {
            _cacheInvalidationService.InvalidateCategoriesCache();
        }

        return result;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
    {
        return await _innerService.GetAllCategoriesAsync();
    }
}
