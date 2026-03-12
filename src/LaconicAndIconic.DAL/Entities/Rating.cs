namespace LaconicAndIconic.DAL.Entities;

public class Rating : BaseEntity
{
    public int RecipeId { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; }

    public Recipe Recipe { get; set; } = null!;
    public User User { get; set; } = null!;
}
