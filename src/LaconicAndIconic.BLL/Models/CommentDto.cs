namespace LaconicAndIconic.BLL.Models;

public class CommentDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
