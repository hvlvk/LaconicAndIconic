namespace LaconicAndIconic.DAL.Entities;

public class UserSubscription
{
    public int FollowerId { get; set; }
    public required ApplicationUser Follower { get; set; }

    public int UserId { get; set; }
    public required ApplicationUser User { get; set; }
}
