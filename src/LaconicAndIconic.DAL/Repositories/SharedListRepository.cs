using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Repositories;

public class SharedListRepository : Repository<SharedList>, ISharedListRepository
{
    public SharedListRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SharedList?> GetWithDetailsAsync(int sharedListId)
    {
        return await Context.Set<SharedList>()
            .Include(sl => sl.Owner)
            .Include(sl => sl.SharedListUsers)
                .ThenInclude(slu => slu.User)
            .Include(sl => sl.SharedListRecipes)
                .ThenInclude(slr => slr.Recipe)
            .FirstOrDefaultAsync(sl => sl.Id == sharedListId);
    }

    public async Task<IEnumerable<SharedList>> GetListsByUserAsync(int userId)
    {
        return await Context.Set<SharedList>()
            .Include(sl => sl.Owner)
            .Include(sl => sl.SharedListUsers)
            .Include(sl => sl.SharedListRecipes)
            .Where(sl => sl.OwnerId == userId || sl.SharedListUsers.Any(slu => slu.UserId == userId))
            .ToListAsync();
    }
}
