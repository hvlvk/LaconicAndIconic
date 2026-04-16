namespace LaconicAndIconic.DAL.Entities;

public class Report : BaseEntity
{
    public int RecipeId { get; set; }
    public int ReporterId { get; set; }
    public int ReportedUserId { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Recipe Recipe { get; set; } = null!;
    public ApplicationUser Reporter { get; set; } = null!;
    public ApplicationUser ReportedUser { get; set; } = null!;
}
