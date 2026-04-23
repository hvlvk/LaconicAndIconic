using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class UserListViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ListType { get; set; } = string.Empty; // "subscriptions" or "followers"
    public bool IsOwnList { get; set; }
    public IEnumerable<UserProfileDto> Users { get; set; } = [];
}

