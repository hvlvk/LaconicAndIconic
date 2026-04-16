using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using System.Globalization;

namespace LaconicAndIconic.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoryService(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
    {
        return await GetAllAsync();
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        });

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

        // Тут теж використовуємо ToUpper і пригнічуємо варнінги
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

        return Result.Success();
    }
}