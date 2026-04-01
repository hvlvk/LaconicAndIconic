namespace LaconicAndIconic.Web.Models;

public class RecipeDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int PrepTimeMin { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsFavorited { get; set; }
}

