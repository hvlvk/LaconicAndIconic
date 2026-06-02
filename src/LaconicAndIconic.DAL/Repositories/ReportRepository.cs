using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Repositories;

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Report>> GetAllWithUsersAsync()
    {
        return await Context.Set<Report>()
            .Include(r => r.Recipe)
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report?> GetByIdWithUsersAsync(int reportId)
    {
        return await Context.Set<Report>()
            .Include(r => r.Recipe)
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .FirstOrDefaultAsync(r => r.Id == reportId);
    }
}
