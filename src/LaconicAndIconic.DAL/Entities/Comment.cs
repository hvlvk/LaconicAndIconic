namespace LaconicAndIconic.DAL.Entities;

public class Comment : BaseEntity
{
    public int RecipeId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;

    public required Recipe Recipe { get; set; }
    public required User Author { get; set; }
}
