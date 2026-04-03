namespace LaconicAndIconic.BLL.Models;

public class UserProfileDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsSubscribed { get; set; }
}
