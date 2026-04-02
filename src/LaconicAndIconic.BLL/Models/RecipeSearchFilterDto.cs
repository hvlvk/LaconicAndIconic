namespace LaconicAndIconic.BLL.Models;

public class RecipeSearchFilterDto
{
    public string SearchTerm { get; set; } = string.Empty;
    public int? CategoryId  { get; set; }
    public string SortBy { get; set; }  = string.Empty;
    public int PageNumber { get; set; }
    public int PageSize { get; set; } = 10;
}
