namespace LaconicAndIconic.DAL.Entities;

public class SharedListRecipe
{
    public int SharedListId { get; set; }
    public required SharedList SharedList { get; set; }

    public int RecipeId { get; set; }
    public required Recipe Recipe { get; set; }
}
