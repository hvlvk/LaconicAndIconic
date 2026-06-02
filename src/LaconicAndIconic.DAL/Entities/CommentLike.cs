namespace LaconicAndIconic.DAL.Entities;

public class CommentLike : BaseEntity
{
    public int CommentId { get; set; }
    public int UserId { get; set; }

    public Comment Comment { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
