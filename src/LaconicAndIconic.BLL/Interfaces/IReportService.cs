using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IReportService
{
    Task<Result> CreateAsync(CreateReportDto dto, int reporterId);
}
