namespace LaconicAndIconic.DAL.Entities;

public class Recipe : BaseEntity
{
    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PrepTimeMin { get; set; }

    public Category Category { get; set; } = null!;
    public User Author { get; set; } = null!;
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
