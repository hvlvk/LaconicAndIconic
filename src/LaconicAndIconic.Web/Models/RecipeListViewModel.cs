
using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class RecipeListViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public string ExternalSearchTerm { get; set; } = string.Empty;
    public int? CategoryId  { get; set; }
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<RecipeDto> Recipes { get; set; } = [];
    public IReadOnlyList<RecipeDto> ExternalRecipes { get; set; } = [];
    public int TotalCount { get; set; }
    public string SortBy { get; set; }  = string.Empty;
    public int PageNumber { get; set; }
    public int PageSize { get; set; } = 10;

    public int TotalPages => TotalCount == 0 ? 1 : (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
