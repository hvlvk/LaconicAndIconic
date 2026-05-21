namespace LaconicAndIconic.DAL.Entities;

public class CommentLike : BaseEntity
{
    public int CommentId { get; set; }
    public int UserId { get; set; }

    public required Comment Comment { get; set; }
    public required ApplicationUser User { get; set; }
}
