using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.BLL.Services;

public class SharedListService : ISharedListService
{
    private readonly IRepository<SharedList> _sharedListRepository;
    private readonly IRepository<SharedListUser> _sharedListUserRepository;
    private readonly IRepository<SharedListRecipe> _sharedListRecipeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<Recipe> _recipeRepository;

    public SharedListService(
        IRepository<SharedList> sharedListRepository,
        IRepository<SharedListUser> sharedListUserRepository,
        IRepository<SharedListRecipe> sharedListRecipeRepository,
        IUserRepository userRepository,
        IRepository<Recipe> recipeRepository)
    {
        _sharedListRepository = sharedListRepository;
        _sharedListUserRepository = sharedListUserRepository;
        _sharedListRecipeRepository = sharedListRecipeRepository;
        _userRepository = userRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<Result<SharedListDto>> CreateAsync(CreateSharedListDto dto, int ownerId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result<SharedListDto>.Failure("Назва обов'язкова");
        }

        var sharedList = new SharedList
        {
            Title = dto.Title,
            Description = dto.Description,
            OwnerId = ownerId
        };

        await _sharedListRepository.AddAsync(sharedList);
        await _sharedListRepository.SaveChangesAsync();

        var owner = await _userRepository.FindByIdAsync(ownerId);

        var result = new SharedListDto
        {
            Id = sharedList.Id,
            Title = sharedList.Title,
            Description = sharedList.Description,
            OwnerId = sharedList.OwnerId,
            OwnerName = owner?.UserName ?? string.Empty,
            MemberCount = 0,
            RecipeCount = 0
        };

        return Result<SharedListDto>.Success(result);
    }

    public async Task<Result> UpdateAsync(int sharedListId, int userId, UpdateSharedListDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result.Failure("Назва обов'язкова");
        }

        var sharedList = await _sharedListRepository.GetByIdAsync(sharedListId);
        if (sharedList == null)
        {
            return Result.Failure("Список не знайдено");
        }

        if (sharedList.OwnerId != userId)
        {
            return Result.Failure("Тільки власник може редагувати список");
        }

        sharedList.Title = dto.Title;
        sharedList.Description = dto.Description;

        _sharedListRepository.Update(sharedList);
        await _sharedListRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int sharedListId, int userId)
    {
        var sharedList = await _sharedListRepository.GetByIdAsync(sharedListId);
        if (sharedList == null)
        {
            return Result.Failure("Список не знайдено");
        }

        if (sharedList.OwnerId != userId)
        {
            return Result.Failure("Тільки власник може видалити список");
        }

        _sharedListRepository.Remove(sharedList);
        await _sharedListRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<SharedListDetailDto>> GetByIdAsync(int sharedListId, int userId)
    {
        var sharedList = await _sharedListRepository.GetQueryable()
            .Include(sl => sl.Owner)
            .Include(sl => sl.SharedListUsers)
                .ThenInclude(slu => slu.User)
            .Include(sl => sl.SharedListRecipes)
                .ThenInclude(slr => slr.Recipe)
            .FirstOrDefaultAsync(sl => sl.Id == sharedListId);

        if (sharedList == null)
        {
            return Result<SharedListDetailDto>.Failure("Список не знайдено");
        }

        if (!IsOwnerOrMember(sharedList, userId))
        {
            return Result<SharedListDetailDto>.Failure("У вас немає доступу до цього списку");
        }

        var dto = new SharedListDetailDto
        {
            Id = sharedList.Id,
            Title = sharedList.Title,
            Description = sharedList.Description,
            OwnerId = sharedList.OwnerId,
            OwnerName = sharedList.Owner.UserName ?? string.Empty,
            Members = sharedList.SharedListUsers.Select(slu => new SharedListMemberDto
            {
                UserId = slu.UserId,
                UserName = slu.User.UserName ?? string.Empty,
                ProfilePicturePath = slu.User.ProfilePicturePath
            }).ToList().AsReadOnly(),
            Recipes = sharedList.SharedListRecipes.Select(slr => new SharedListRecipeItemDto
            {
                RecipeId = slr.RecipeId,
                RecipeTitle = slr.Recipe.Title,
                RecipeImagePath = slr.Recipe.ImagePath
            }).ToList().AsReadOnly()
        };

        return Result<SharedListDetailDto>.Success(dto);
    }

    public async Task<Result<IEnumerable<SharedListDto>>> GetListsByUserAsync(int userId)
    {
        var lists = await _sharedListRepository.GetQueryable()
            .Include(sl => sl.Owner)
            .Include(sl => sl.SharedListUsers)
            .Include(sl => sl.SharedListRecipes)
            .Where(sl => sl.OwnerId == userId || sl.SharedListUsers.Any(slu => slu.UserId == userId))
            .ToListAsync();

        var dtos = lists.Select(sl => new SharedListDto
        {
            Id = sl.Id,
            Title = sl.Title,
            Description = sl.Description,
            OwnerId = sl.OwnerId,
            OwnerName = sl.Owner.UserName ?? string.Empty,
            MemberCount = sl.SharedListUsers.Count,
            RecipeCount = sl.SharedListRecipes.Count
        });

        return Result<IEnumerable<SharedListDto>>.Success(dtos);
    }

    public async Task<Result> InviteUserAsync(int sharedListId, int ownerId, string username)
    {
        ArgumentNullException.ThrowIfNull(username);

        var sharedList = await _sharedListRepository.GetByIdAsync(sharedListId);
        if (sharedList == null)
        {
            return Result.Failure("Список не знайдено");
        }

        if (sharedList.OwnerId != ownerId)
        {
            return Result.Failure("Тільки власник може запрошувати користувачів");
        }

        var targetUser = await _userRepository.FindByUserNameAsync(username);
        if (targetUser == null)
        {
            return Result.Failure("Користувача не знайдено");
        }

        if (targetUser.Id == ownerId)
        {
            return Result.Failure("Власник вже є учасником списку");
        }

        var alreadyMember = await _sharedListUserRepository.AnyAsync(
            slu => slu.SharedListId == sharedListId && slu.UserId == targetUser.Id);
        if (alreadyMember)
        {
            return Result.Failure("Користувач вже є учасником списку");
        }

        var membership = new SharedListUser
        {
            SharedListId = sharedListId,
            UserId = targetUser.Id
        };

        await _sharedListUserRepository.AddAsync(membership);
        await _sharedListUserRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> RemoveUserAsync(int sharedListId, int ownerId, int targetUserId)
    {
        var sharedList = await _sharedListRepository.GetByIdAsync(sharedListId);
        if (sharedList == null)
        {
            return Result.Failure("Список не знайдено");
        }

        if (sharedList.OwnerId != ownerId)
        {
            return Result.Failure("Тільки власник може видаляти учасників");
        }

        if (targetUserId == ownerId)
        {
            return Result.Failure("Власник не може видалити себе зі списку");
        }

        var membership = await _sharedListUserRepository.GetQueryable()
            .FirstOrDefaultAsync(slu => slu.SharedListId == sharedListId && slu.UserId == targetUserId);

        if (membership == null)
        {
            return Result.Failure("Користувач не є учасником списку");
        }

        _sharedListUserRepository.Remove(membership);
        await _sharedListUserRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AddRecipeAsync(int sharedListId, int userId, int recipeId)
    {
        var sharedList = await _sharedListRepository.GetQueryable()
            .Include(sl => sl.SharedListUsers)
            .FirstOrDefaultAsync(sl => sl.Id == sharedListId);

        if (sharedList == null)
        {
            return Result.Failure("Список не знайдено");
        }

        if (!IsOwnerOrMember(sharedList, userId))
        {
            return Result.Failure("У вас немає доступу до цього списку");
        }

        var recipeExists = await _recipeRepository.ExistsAsync(recipeId);
        if (!recipeExists)
        {
            return Result.Failure("Рецепт не знайдено");
        }

        var alreadyAdded = await _sharedListRecipeRepository.AnyAsync(
            slr => slr.SharedListId == sharedListId && slr.RecipeId == recipeId);
        if (alreadyAdded)
        {
            return Result.Failure("Рецепт вже додано до списку");
        }

        var entry = new SharedListRecipe
        {
            SharedListId = sharedListId,
            RecipeId = recipeId
        };

        await _sharedListRecipeRepository.AddAsync(entry);
        await _sharedListRecipeRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> RemoveRecipeAsync(int sharedListId, int userId, int recipeId)
    {
        var sharedList = await _sharedListRepository.GetQueryable()
            .Include(sl => sl.SharedListUsers)
            .FirstOrDefaultAsync(sl => sl.Id == sharedListId);

        if (sharedList == null)
        {
            return Result.Failure("Список не знайдено");
        }

        if (!IsOwnerOrMember(sharedList, userId))
        {
            return Result.Failure("У вас немає доступу до цього списку");
        }

        var entry = await _sharedListRecipeRepository.GetQueryable()
            .FirstOrDefaultAsync(slr => slr.SharedListId == sharedListId && slr.RecipeId == recipeId);

        if (entry == null)
        {
            return Result.Failure("Рецепт не знайдено у списку");
        }

        _sharedListRecipeRepository.Remove(entry);
        await _sharedListRecipeRepository.SaveChangesAsync();

        return Result.Success();
    }

    private static bool IsOwnerOrMember(SharedList sharedList, int userId)
    {
        return sharedList.OwnerId == userId ||
               sharedList.SharedListUsers.Any(slu => slu.UserId == userId);
    }
}
