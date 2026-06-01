namespace LaconicAndIconic.DAL.Entities;

public class Report : BaseEntity
{
    public int RecipeId { get; set; }
    public int ReporterId { get; set; }
    public int ReportedUserId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public required Recipe Recipe { get; set; }
    public required ApplicationUser Reporter { get; set; }
    public required ApplicationUser ReportedUser { get; set; }
}
