namespace LaconicAndIconic.DAL.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
