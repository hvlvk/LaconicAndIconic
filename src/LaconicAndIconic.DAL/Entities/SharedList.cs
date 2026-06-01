namespace LaconicAndIconic.DAL.Entities;

public class SharedList : BaseEntity
{
    public int OwnerId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public required ApplicationUser Owner { get; set; }

    public ICollection<SharedListUser> SharedListUsers { get; } = [];
    public ICollection<SharedListRecipe> SharedListRecipes { get; } = [];
}
