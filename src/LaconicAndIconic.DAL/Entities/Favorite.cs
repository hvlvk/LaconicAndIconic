namespace LaconicAndIconic.DAL.Entities;

public class Favorite : BaseEntity
{
    public int UserId { get; set; }
    public int RecipeId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;
}
