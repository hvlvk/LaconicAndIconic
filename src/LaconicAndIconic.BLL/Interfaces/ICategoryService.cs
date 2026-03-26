using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
}