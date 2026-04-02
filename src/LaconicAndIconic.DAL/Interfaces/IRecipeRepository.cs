using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<RecipeSearchResult> SearchAsync(RecipeSearchFilter filter);
}

public class RecipeSearchFilter
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class RecipeSearchResult
{
    public IReadOnlyList<Recipe> Recipes { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; }
}


