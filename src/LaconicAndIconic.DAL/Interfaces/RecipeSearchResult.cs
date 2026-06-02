using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

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
