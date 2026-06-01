namespace LaconicAndIconic.DAL.Entities;

public class Recipe : BaseEntity
{
    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int PrepTimeMin { get; set; }
    public int Servings { get; set; }
    public string Ingredients { get; set; } = string.Empty;
    public string CookingMethod { get; set; } = string.Empty;
    public string? ExternalSource { get; set; }
    public string? ExternalId { get; set; }

    public required Category Category { get; set; }
    public required ApplicationUser Author { get; set; }
    public ICollection<Rating> Ratings { get; } = [];
    public ICollection<Comment> Comments { get; } = [];
    public ICollection<SharedListRecipe> SharedListRecipes { get; } = [];
}
