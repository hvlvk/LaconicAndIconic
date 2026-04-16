namespace LaconicAndIconic.Web.Models;

public class SharedListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int RecipeCount { get; set; }
}
