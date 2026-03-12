namespace LaconicAndIconic.DAL.Entities;

public class Comment : BaseEntity
{
    public int RecipeId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;

    public Recipe Recipe { get; set; } = null!;
    public User Author { get; set; } = null!;
}
