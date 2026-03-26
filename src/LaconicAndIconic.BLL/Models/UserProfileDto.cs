namespace LaconicAndIconic.BLL.Models;

public class UserProfileDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
}
