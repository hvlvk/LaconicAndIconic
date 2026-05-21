namespace LaconicAndIconic.BLL.Models;

public class ReportDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string RecipeTitle { get; set; } = string.Empty;
    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public int ReportedUserId { get; set; }
    public string ReportedUserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
