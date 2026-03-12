namespace LaconicAndIconic.DAL.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public ICollection<Recipe> Recipes { get; set; } = [];
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
