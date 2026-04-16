namespace LaconicAndIconic.BLL.Models;

public class EditCommentDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string Content { get; set; } = string.Empty;
}
