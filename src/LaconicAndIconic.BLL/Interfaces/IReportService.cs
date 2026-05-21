using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IReportService
{
    Task<Result> CreateAsync(CreateReportDto dto, int reporterId);
    Task<Result<IEnumerable<ReportDto>>> GetAllAsync();
    Task<Result<ReportDto>> GetByIdAsync(int reportId);
    Task<Result> DismissAsync(int reportId);
}
