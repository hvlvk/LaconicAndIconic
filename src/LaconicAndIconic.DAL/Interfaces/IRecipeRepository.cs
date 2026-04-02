using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<(IEnumerable<Recipe> Recipes, int TotalCount)> SearchAsync(
        string? searchTerm,
        int? categoryId,
        string? sortBy,
        int pageNumber,
        int pageSize);
}
