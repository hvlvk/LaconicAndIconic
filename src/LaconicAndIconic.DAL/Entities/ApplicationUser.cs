using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Recipe> Recipes { get; } = [];
    public ICollection<Rating> Ratings { get; } = [];
    public ICollection<Comment> Comments { get; } = [];
}
