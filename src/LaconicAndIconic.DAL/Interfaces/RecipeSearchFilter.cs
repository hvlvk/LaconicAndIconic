namespace LaconicAndIconic.DAL.Interfaces;

public class RecipeSearchFilter
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
