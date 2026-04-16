using System.ComponentModel.DataAnnotations;

namespace LaconicAndIconic.Web.Models;

public class ReportRecipeViewModel
{
    public int RecipeId { get; set; }
    public int ReportedUserId { get; set; }

    [Display(Name = "Користувач")]
    public string ReportedUserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть причину скарги")]
    [Display(Name = "Причина скарги")]
    public string Reason { get; set; } = string.Empty;
}
