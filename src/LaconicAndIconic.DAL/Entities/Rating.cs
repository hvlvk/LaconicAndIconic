namespace LaconicAndIconic.DAL.Entities;

public class Rating : BaseEntity
{
    public int RecipeId { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; }

    public required Recipe Recipe { get; set; }
    public required User User { get; set; }
}
