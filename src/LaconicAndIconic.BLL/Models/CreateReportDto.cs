namespace LaconicAndIconic.BLL.Models;

public class CreateReportDto
{
    public int RecipeId { get; set; }
    public int ReportedUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
