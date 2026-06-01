namespace LaconicAndIconic.DAL.Entities;

public class SharedListUser
{
    public int SharedListId { get; set; }
    public required SharedList SharedList { get; set; }

    public int UserId { get; set; }
    public required ApplicationUser User { get; set; }
}
