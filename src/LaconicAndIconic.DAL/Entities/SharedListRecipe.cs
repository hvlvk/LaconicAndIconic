namespace LaconicAndIconic.DAL.Entities;

public class SharedListRecipe
{
    public int SharedListId { get; set; }
    public SharedList SharedList { get; set; } = null!;

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}
