using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.Web.Models;

public class AdminRecipeReportsViewModel
{
    public IReadOnlyCollection<ReportDto> Reports { get; set; } = Array.Empty<ReportDto>();
}
