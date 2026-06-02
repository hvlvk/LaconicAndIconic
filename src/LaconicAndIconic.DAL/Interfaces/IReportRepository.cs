using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IReportRepository : IRepository<Report>
{
    Task<IEnumerable<Report>> GetAllWithUsersAsync();
    Task<Report?> GetByIdWithUsersAsync(int reportId);
}
