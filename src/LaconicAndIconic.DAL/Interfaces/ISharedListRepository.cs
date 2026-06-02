using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

public interface ISharedListRepository: IRepository<SharedList>
{
    Task<SharedList?> GetWithDetailsAsync(int sharedListId);
    Task<IEnumerable<SharedList>> GetListsByUserAsync(int userId);
}
