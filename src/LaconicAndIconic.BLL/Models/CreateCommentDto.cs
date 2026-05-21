namespace LaconicAndIconic.BLL.Models;

public class CreateCommentDto
{
    public int RecipeId { get; set; }
    public string Content { get; set; } = string.Empty;
}
