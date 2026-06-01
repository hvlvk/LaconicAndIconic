namespace LaconicAndIconic.DAL.Entities;

public class UserSubscription
{
    public int FollowerId { get; set; }
    public ApplicationUser Follower { get; set; } = null!;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
