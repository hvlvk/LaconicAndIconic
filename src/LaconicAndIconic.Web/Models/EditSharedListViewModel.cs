using System.ComponentModel.DataAnnotations;

namespace LaconicAndIconic.Web.Models;

public class EditSharedListViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва обов'язкова")]
    [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів")]
    [Display(Name = "Назва")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
    [Display(Name = "Опис")]
    public string? Description { get; set; }
}
