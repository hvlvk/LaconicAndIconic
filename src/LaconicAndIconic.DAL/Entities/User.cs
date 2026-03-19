using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Entities;

public class User : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Recipe> Recipes { get; set; } = [];
    public ICollection<Rating> Ratings { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
