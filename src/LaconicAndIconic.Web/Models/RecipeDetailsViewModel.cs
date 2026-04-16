namespace LaconicAndIconic.Web.Models;

public class RecipeDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int PrepTimeMin { get; set; }
    public int Servings { get; set; }
    public string Ingredients { get; set; } = string.Empty;
    public string CookingMethod { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int? CurrentUserRating { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorProfilePicturePath { get; set; }
    public bool IsSubscribedToAuthor { get; set; }
}

