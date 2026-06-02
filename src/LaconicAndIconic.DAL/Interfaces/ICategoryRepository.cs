using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> CategoryExistsByNameAsync(string name, int? excludeId = null);
}
