using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class AdminUsersViewModel
{
    public IReadOnlyList<UserProfileDto> Users { get; set; } = new List<UserProfileDto>();
}
