using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class RecipeDetailsViewModel
{
    public RecipeDto Recipe { get; set; } = null!;
    public bool IsFavorited { get; set; }
}
