namespace LaconicAndIconic.DAL.Entities;

public class SharedListUser
{
    public int SharedListId { get; set; }
    public SharedList SharedList { get; set; } = null!;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}