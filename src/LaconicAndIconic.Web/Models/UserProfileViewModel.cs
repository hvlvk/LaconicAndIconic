namespace LaconicAndIconic.Web.Models;

public class UserProfileViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public bool IsOwnProfile { get; set; }
}
