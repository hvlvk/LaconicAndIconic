using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.BLL.Services;

public class CategoryService : ICategoryService
{
    private const string AllCategoriesCacheKey = "all_categories";
    private readonly IRepository<Category> _categoryRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly TimeSpan _cacheDuration;

    public CategoryService(
        IRepository<Category> categoryRepository,
        IMemoryCache memoryCache,
        ICacheInvalidationService cacheInvalidationService,
        IOptions<CachingOptions> cachingOptions)
    {
        _categoryRepository = categoryRepository;
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _cacheInvalidationService = cacheInvalidationService ?? throw new ArgumentNullException(nameof(cacheInvalidationService));
        ArgumentNullException.ThrowIfNull(cachingOptions);
        _cacheDuration = TimeSpan.FromMinutes(cachingOptions.Value.CategoriesCacheLifetimeMinutes);
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
    {
        return await GetAllAsync();
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        if (_memoryCache.TryGetValue(AllCategoriesCacheKey, out IEnumerable<CategoryDto>? cachedCategories))
        {
            return Result<IEnumerable<CategoryDto>>.Success(cachedCategories!);
        }

        var categories = await _categoryRepository.GetAllAsync();
        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        _memoryCache.Set(AllCategoriesCacheKey, dtos, _cacheDuration);

        return Result<IEnumerable<CategoryDto>>.Success(dtos);
    }

    public async Task<Result> CreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure("Назва категорії не може бути порожньою");
        }

        name = name.Trim();

#pragma warning disable CA1862, CA1304, CA1311, RCS1155, CA1308
        var exists = await _categoryRepository.AnyAsync(c =>
            c.Name.ToUpper() == name.ToUpper());
#pragma warning restore CA1862, CA1304, CA1311, RCS1155, CA1308

        if (exists)
        {
            return Result.Failure("Категорія з такою назвою вже існує");
        }

        await _categoryRepository.AddAsync(new Category { Name = name });
        await _categoryRepository.SaveChangesAsync();

        _cacheInvalidationService.InvalidateCategoriesCache();

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(int id, string name)
    {
        if (id <= 0)
        {
            return Result.Failure("Невірний ідентифікатор категорії.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure("Назва категорії не може бути порожньою");
        }

        name = name.Trim();
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return Result.Failure("Категорію не знайдено.");
        }

#pragma warning disable CA1862, CA1304, CA1311, RCS1155, CA1308
        var exists = await _categoryRepository.AnyAsync(c =>
            c.Id != id && c.Name.ToUpper() == name.ToUpper());
#pragma warning restore CA1862, CA1304, CA1311, RCS1155, CA1308

        if (exists)
        {
            return Result.Failure("Категорія з такою назвою вже існує");
        }

        category.Name = name;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        _cacheInvalidationService.InvalidateCategoriesCache();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("Невірний ідентифікатор категорії.");
        }

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return Result.Failure("Категорію не знайдено.");
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync();

        _cacheInvalidationService.InvalidateCategoriesCache();

        return Result.Success();
    }
}
