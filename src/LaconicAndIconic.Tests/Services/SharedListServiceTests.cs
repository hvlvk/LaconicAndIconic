using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace LaconicAndIconic.Tests.Services;

public class SharedListServiceTests
{
    private readonly Mock<IRepository<SharedList>> _sharedListRepoMock;
    private readonly Mock<IRepository<SharedListUser>> _sharedListUserRepoMock;
    private readonly Mock<IRepository<SharedListRecipe>> _sharedListRecipeRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRepository<Recipe>> _recipeRepoMock;
    private readonly SharedListService _service;

    public SharedListServiceTests()
    {
        _sharedListRepoMock = new Mock<IRepository<SharedList>>();
        _sharedListUserRepoMock = new Mock<IRepository<SharedListUser>>();
        _sharedListRecipeRepoMock = new Mock<IRepository<SharedListRecipe>>();
        _userRepoMock = new Mock<IUserRepository>();
        _recipeRepoMock = new Mock<IRepository<Recipe>>();

        _service = new SharedListService(
            _sharedListRepoMock.Object,
            _sharedListUserRepoMock.Object,
            _sharedListRecipeRepoMock.Object,
            _userRepoMock.Object,
            _recipeRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateSharedListDto { Title = "Weekend Meals", Description = "Family favorites" };
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };

        _sharedListRepoMock.Setup(r => r.AddAsync(It.IsAny<SharedList>())).Returns(Task.CompletedTask);
        _sharedListRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(owner);

        // Act
        var result = await _service.CreateAsync(dto, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Weekend Meals", result.Value.Title);
        Assert.Equal("Family favorites", result.Value.Description);
        Assert.Equal(1, result.Value.OwnerId);
        Assert.Equal("chef_anna", result.Value.OwnerName);
        Assert.Equal(0, result.Value.MemberCount);
        Assert.Equal(0, result.Value.RecipeCount);
        _sharedListRepoMock.Verify(r => r.AddAsync(It.IsAny<SharedList>()), Times.Once);
        _sharedListRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateSharedListDto { Title = "  " };

        // Act
        var result = await _service.CreateAsync(dto, 1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Назва обов'язкова", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_OwnerUpdates_ReturnsSuccess()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "Old Title", OwnerId = 5 };
        var dto = new UpdateSharedListDto { Title = "New Title", Description = "Updated description" };

        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);
        _sharedListRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(1, 5, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", sharedList.Title);
        Assert.Equal("Updated description", sharedList.Description);
        _sharedListRepoMock.Verify(r => r.Update(sharedList), Times.Once);
        _sharedListRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonOwner_ReturnsFailure()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "My List", OwnerId = 5 };
        var dto = new UpdateSharedListDto { Title = "Hacked Title" };

        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);

        // Act
        var result = await _service.UpdateAsync(1, 99, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Тільки власник може редагувати список", result.ErrorMessage);
        _sharedListRepoMock.Verify(r => r.Update(It.IsAny<SharedList>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateSharedListDto { Title = "Some Title" };
        _sharedListRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((SharedList?)null);

        // Act
        var result = await _service.UpdateAsync(999, 1, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Список не знайдено", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_OwnerDeletes_ReturnsSuccess()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "To Delete", OwnerId = 3 };
        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);
        _sharedListRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1, 3);

        // Assert
        Assert.True(result.IsSuccess);
        _sharedListRepoMock.Verify(r => r.Remove(sharedList), Times.Once);
        _sharedListRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonOwner_ReturnsFailure()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "Protected List", OwnerId = 3 };
        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);

        // Act
        var result = await _service.DeleteAsync(1, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Тільки власник може видалити список", result.ErrorMessage);
        _sharedListRepoMock.Verify(r => r.Remove(It.IsAny<SharedList>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_OwnerAccess_ReturnsDetail()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var recipe = new Recipe { Id = 10, Title = "Pasta Carbonara", ImagePath = "/images/pasta.jpg", CategoryId = 1, AuthorId = 1 };
        var sharedList = new SharedList
        {
            Id = 1,
            Title = "Italian Recipes",
            Description = "Best Italian dishes",
            OwnerId = 1,
            Owner = owner
        };
        sharedList.SharedListRecipes.Add(new SharedListRecipe { SharedListId = 1, RecipeId = 10, Recipe = recipe });

        var lists = new List<SharedList> { sharedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetByIdAsync(1, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Italian Recipes", result.Value.Title);
        Assert.Equal("Best Italian dishes", result.Value.Description);
        Assert.Equal(1, result.Value.OwnerId);
        Assert.Equal("chef_anna", result.Value.OwnerName);
        Assert.Single(result.Value.Recipes);
        Assert.Equal("Pasta Carbonara", result.Value.Recipes[0].RecipeTitle);
    }

    [Fact]
    public async Task GetByIdAsync_MemberAccess_ReturnsDetail()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var member = new ApplicationUser { Id = 2, UserName = "bob_cook" };
        var sharedList = new SharedList
        {
            Id = 1,
            Title = "Shared Favorites",
            OwnerId = 1,
            Owner = owner
        };
        sharedList.SharedListUsers.Add(new SharedListUser { SharedListId = 1, UserId = 2, User = member });

        var lists = new List<SharedList> { sharedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetByIdAsync(1, 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Shared Favorites", result.Value.Title);
        Assert.Single(result.Value.Members);
        Assert.Equal("bob_cook", result.Value.Members[0].UserName);
    }

    [Fact]
    public async Task GetByIdAsync_NonMember_ReturnsFailure()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var sharedList = new SharedList
        {
            Id = 1,
            Title = "Private List",
            OwnerId = 1,
            Owner = owner
        };

        var lists = new List<SharedList> { sharedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetByIdAsync(1, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("У вас немає доступу до цього списку", result.ErrorMessage);
    }

    [Fact]
    public async Task GetListsByUserAsync_ReturnsOwnedAndMemberLists()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var otherOwner = new ApplicationUser { Id = 2, UserName = "bob_cook" };

        var ownedList = new SharedList { Id = 1, Title = "My List", OwnerId = 1, Owner = owner };
        var memberList = new SharedList { Id = 2, Title = "Bob's List", OwnerId = 2, Owner = otherOwner };
        memberList.SharedListUsers.Add(new SharedListUser { SharedListId = 2, UserId = 1 });

        var unrelatedList = new SharedList { Id = 3, Title = "Unrelated", OwnerId = 3, Owner = new ApplicationUser { Id = 3, UserName = "stranger" } };

        var lists = new List<SharedList> { ownedList, memberList, unrelatedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetListsByUserAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var dtos = result.Value.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.Contains(dtos, d => d.Title == "My List");
        Assert.Contains(dtos, d => d.Title == "Bob's List");
    }

    [Fact]
    public async Task InviteUserAsync_OwnerInvites_ReturnsSuccess()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "Cooking Club", OwnerId = 1 };
        var targetUser = new ApplicationUser { Id = 5, UserName = "new_member" };

        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);
        _userRepoMock.Setup(r => r.FindByUserNameAsync("new_member")).ReturnsAsync(targetUser);
        _sharedListUserRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<SharedListUser, bool>>>())).ReturnsAsync(false);
        _sharedListUserRepoMock.Setup(r => r.AddAsync(It.IsAny<SharedListUser>())).Returns(Task.CompletedTask);
        _sharedListUserRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.InviteUserAsync(1, 1, "new_member");

        // Assert
        Assert.True(result.IsSuccess);
        _sharedListUserRepoMock.Verify(r => r.AddAsync(It.Is<SharedListUser>(
            slu => slu.SharedListId == 1 && slu.UserId == 5)), Times.Once);
        _sharedListUserRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task InviteUserAsync_NonOwner_ReturnsFailure()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "Not Yours", OwnerId = 1 };
        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);

        // Act
        var result = await _service.InviteUserAsync(1, 99, "someone");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Тільки власник може запрошувати користувачів", result.ErrorMessage);
        _sharedListUserRepoMock.Verify(r => r.AddAsync(It.IsAny<SharedListUser>()), Times.Never);
    }

    [Fact]
    public async Task InviteUserAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "My List", OwnerId = 1 };
        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);
        _userRepoMock.Setup(r => r.FindByUserNameAsync("ghost_user")).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.InviteUserAsync(1, 1, "ghost_user");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено", result.ErrorMessage);
    }

    [Fact]
    public async Task InviteUserAsync_AlreadyMember_ReturnsFailure()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "My List", OwnerId = 1 };
        var existingUser = new ApplicationUser { Id = 5, UserName = "already_here" };

        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);
        _userRepoMock.Setup(r => r.FindByUserNameAsync("already_here")).ReturnsAsync(existingUser);
        _sharedListUserRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<SharedListUser, bool>>>())).ReturnsAsync(true);

        // Act
        var result = await _service.InviteUserAsync(1, 1, "already_here");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Користувач вже є учасником списку", result.ErrorMessage);
        _sharedListUserRepoMock.Verify(r => r.AddAsync(It.IsAny<SharedListUser>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUserAsync_OwnerRemovesMember_ReturnsSuccess()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "My List", OwnerId = 1 };
        var membership = new SharedListUser { SharedListId = 1, UserId = 5 };

        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);

        var memberships = new List<SharedListUser> { membership };
        var mockDbSet = memberships.AsQueryable().BuildMockDbSet();
        _sharedListUserRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);
        _sharedListUserRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.RemoveUserAsync(1, 1, 5);

        // Assert
        Assert.True(result.IsSuccess);
        _sharedListUserRepoMock.Verify(r => r.Remove(membership), Times.Once);
        _sharedListUserRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveUserAsync_NonOwner_ReturnsFailure()
    {
        // Arrange
        var sharedList = new SharedList { Id = 1, Title = "Protected", OwnerId = 1 };
        _sharedListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sharedList);

        // Act
        var result = await _service.RemoveUserAsync(1, 99, 5);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Тільки власник може видаляти учасників", result.ErrorMessage);
        _sharedListUserRepoMock.Verify(r => r.Remove(It.IsAny<SharedListUser>()), Times.Never);
    }

    [Fact]
    public async Task AddRecipeAsync_MemberAdds_ReturnsSuccess()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var member = new ApplicationUser { Id = 2, UserName = "bob_cook" };
        var sharedList = new SharedList { Id = 1, Title = "Shared Recipes", OwnerId = 1, Owner = owner };
        sharedList.SharedListUsers.Add(new SharedListUser { SharedListId = 1, UserId = 2, User = member });

        var lists = new List<SharedList> { sharedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        _recipeRepoMock.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _sharedListRecipeRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<SharedListRecipe, bool>>>())).ReturnsAsync(false);
        _sharedListRecipeRepoMock.Setup(r => r.AddAsync(It.IsAny<SharedListRecipe>())).Returns(Task.CompletedTask);
        _sharedListRecipeRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddRecipeAsync(1, 2, 10);

        // Assert
        Assert.True(result.IsSuccess);
        _sharedListRecipeRepoMock.Verify(r => r.AddAsync(It.Is<SharedListRecipe>(
            slr => slr.SharedListId == 1 && slr.RecipeId == 10)), Times.Once);
        _sharedListRecipeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddRecipeAsync_NonMember_ReturnsFailure()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var sharedList = new SharedList { Id = 1, Title = "Private List", OwnerId = 1, Owner = owner };

        var lists = new List<SharedList> { sharedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        // Act
        var result = await _service.AddRecipeAsync(1, 99, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("У вас немає доступу до цього списку", result.ErrorMessage);
        _sharedListRecipeRepoMock.Verify(r => r.AddAsync(It.IsAny<SharedListRecipe>()), Times.Never);
    }

    [Fact]
    public async Task AddRecipeAsync_DuplicateRecipe_ReturnsFailure()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var sharedList = new SharedList { Id = 1, Title = "My Recipes", OwnerId = 1, Owner = owner };

        var lists = new List<SharedList> { sharedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        _recipeRepoMock.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _sharedListRecipeRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<SharedListRecipe, bool>>>())).ReturnsAsync(true);

        // Act
        var result = await _service.AddRecipeAsync(1, 1, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Рецепт вже додано до списку", result.ErrorMessage);
        _sharedListRecipeRepoMock.Verify(r => r.AddAsync(It.IsAny<SharedListRecipe>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRecipeAsync_MemberRemoves_ReturnsSuccess()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var member = new ApplicationUser { Id = 2, UserName = "bob_cook" };
        var sharedList = new SharedList { Id = 1, Title = "Shared Recipes", OwnerId = 1, Owner = owner };
        sharedList.SharedListUsers.Add(new SharedListUser { SharedListId = 1, UserId = 2, User = member });

        var lists = new List<SharedList> { sharedList };
        var mockListDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockListDbSet.Object);

        var entry = new SharedListRecipe { SharedListId = 1, RecipeId = 10 };
        var entries = new List<SharedListRecipe> { entry };
        var mockRecipeDbSet = entries.AsQueryable().BuildMockDbSet();
        _sharedListRecipeRepoMock.Setup(r => r.GetQueryable()).Returns(mockRecipeDbSet.Object);
        _sharedListRecipeRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.RemoveRecipeAsync(1, 2, 10);

        // Assert
        Assert.True(result.IsSuccess);
        _sharedListRecipeRepoMock.Verify(r => r.Remove(entry), Times.Once);
        _sharedListRecipeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveRecipeAsync_NonMember_ReturnsFailure()
    {
        // Arrange
        var owner = new ApplicationUser { Id = 1, UserName = "chef_anna" };
        var sharedList = new SharedList { Id = 1, Title = "Private List", OwnerId = 1, Owner = owner };

        var lists = new List<SharedList> { sharedList };
        var mockDbSet = lists.AsQueryable().BuildMockDbSet();
        _sharedListRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        // Act
        var result = await _service.RemoveRecipeAsync(1, 99, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("У вас немає доступу до цього списку", result.ErrorMessage);
        _sharedListRecipeRepoMock.Verify(r => r.Remove(It.IsAny<SharedListRecipe>()), Times.Never);
    }
}
