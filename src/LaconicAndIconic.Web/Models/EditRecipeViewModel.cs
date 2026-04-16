using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LaconicAndIconic.Web.Models;

public class EditRecipeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва обов'язкова")]
    [Display(Name = "Назва")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Опис обов'язковий")]
    [Display(Name = "Опис")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Час приготування має бути більше 0")]
    [Display(Name = "Час приготування (хвилин)")]
    public int PrepTimeMin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Кількість порцій має бути більше 0")]
    [Display(Name = "Кількість порцій")]
    public int Servings { get; set; }

    [Required(ErrorMessage = "Інгредієнти обов'язкові")]
    [Display(Name = "Інгредієнти")]
    public string Ingredients { get; set; } = string.Empty;

    [Required(ErrorMessage = "Спосіб приготування обов'язковий")]
    [Display(Name = "Спосіб приготування")]
    public string CookingMethod { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть категорію")]
    [Display(Name = "Категорія")]
    public int CategoryId { get; set; }

    [Display(Name = "Фото страви")]
    public IFormFile? Image { get; set; }

    public string? CurrentImagePath { get; set; }
}

