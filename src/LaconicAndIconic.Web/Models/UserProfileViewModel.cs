using System.Collections.Generic;
using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class UserProfileViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public bool IsOwnProfile { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsSubscribed { get; set; }
    public IEnumerable<RecipeDto> Recipes { get; set; } = [];
}
