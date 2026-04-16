using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class AdminCategoriesViewModel
{
    public string NewCategoryName { get; set; } = string.Empty;
    public IReadOnlyList<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}
