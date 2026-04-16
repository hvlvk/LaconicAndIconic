using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
    Task<Result<IEnumerable<CategoryDto>>> GetAllAsync();
    Task<Result> CreateAsync(string name);
    Task<Result> UpdateAsync(int id, string name);
    Task<Result> DeleteAsync(int id);
}