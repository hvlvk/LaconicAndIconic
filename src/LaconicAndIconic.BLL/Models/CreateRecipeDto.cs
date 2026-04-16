using Microsoft.AspNetCore.Http;

namespace LaconicAndIconic.BLL.Models;

public class CreateRecipeDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PrepTimeMin { get; set; }
    public int Servings { get; set; }
    public string Ingredients { get; set; } = string.Empty;
    public string CookingMethod { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; }
}
