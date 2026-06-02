using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Models;

public class UserProfileProjection
{
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsSubscribed { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
