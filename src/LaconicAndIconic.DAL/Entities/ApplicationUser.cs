using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.DAL.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ProfilePicturePath { get; set; }

    public ICollection<Recipe> Recipes { get; } = [];
    public ICollection<Rating> Ratings { get; } = [];
    public ICollection<Comment> Comments { get; } = [];
    
    public ICollection<UserSubscription> Followers { get; } = [];
    public ICollection<UserSubscription> Following { get; } = [];
}
