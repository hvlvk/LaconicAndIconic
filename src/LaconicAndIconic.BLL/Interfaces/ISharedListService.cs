using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface ISharedListService
{
    Task<Result<SharedListDto>> CreateAsync(CreateSharedListDto dto, int ownerId);
    Task<Result> UpdateAsync(int sharedListId, int userId, UpdateSharedListDto dto);
    Task<Result> DeleteAsync(int sharedListId, int userId);
    Task<Result<SharedListDetailDto>> GetByIdAsync(int sharedListId, int userId);
    Task<Result<IEnumerable<SharedListDto>>> GetListsByUserAsync(int userId);
    Task<Result> InviteUserAsync(int sharedListId, int ownerId, string username);
    Task<Result> RemoveUserAsync(int sharedListId, int ownerId, int targetUserId);
    Task<Result> AddRecipeAsync(int sharedListId, int userId, int recipeId);
    Task<Result> RemoveRecipeAsync(int sharedListId, int userId, int recipeId);
}
