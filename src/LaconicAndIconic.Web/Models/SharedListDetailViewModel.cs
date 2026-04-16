using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class SharedListDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public IReadOnlyList<SharedListMemberDto> Members { get; set; } = [];
    public IReadOnlyList<SharedListRecipeItemDto> Recipes { get; set; } = [];
}
