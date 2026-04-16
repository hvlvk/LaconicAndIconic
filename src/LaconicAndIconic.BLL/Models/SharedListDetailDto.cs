namespace LaconicAndIconic.BLL.Models;

public class SharedListDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public IReadOnlyList<SharedListMemberDto> Members { get; set; } = [];
    public IReadOnlyList<SharedListRecipeItemDto> Recipes { get; set; } = [];
}

public class SharedListMemberDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class SharedListRecipeItemDto
{
    public int RecipeId { get; set; }
    public string RecipeTitle { get; set; } = string.Empty;
    public string? RecipeImagePath { get; set; }
}
