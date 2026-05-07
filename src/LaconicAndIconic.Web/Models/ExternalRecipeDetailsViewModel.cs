namespace LaconicAndIconic.Web.Models;

public class ExternalRecipeDetailsViewModel
{
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string Ingredients { get; set; } = string.Empty;
    public string CookingMethod { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = "TheMealDB";
}
