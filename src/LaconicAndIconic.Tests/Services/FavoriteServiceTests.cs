using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Moq;
using System.Linq.Expressions;

namespace LaconicAndIconic.Tests.Services;

public class FavoriteServiceTests
{
    private readonly Mock<IRepository<Favorite>> _favoriteRepositoryMock;
    private readonly Mock<IRepository<Recipe>> _recipeRepositoryMock;
    private readonly FavoriteService _service;

    public FavoriteServiceTests()
    {
        _favoriteRepositoryMock = new Mock<IRepository<Favorite>>();
        _recipeRepositoryMock = new Mock<IRepository<Recipe>>();
        _service = new FavoriteService(_favoriteRepositoryMock.Object, _recipeRepositoryMock.Object);
    }

    [Fact]
    public async Task AddFavoriteAsync_NewFavorite_AddsAndReturnsSuccess()
    {
        // Arrange
        _favoriteRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Expression<Func<Favorite, object>>[]>()))
            .ReturnsAsync([]);

        _recipeRepositoryMock
            .Setup(r => r.ExistsAsync(10))
            .ReturnsAsync(true);

        _favoriteRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Favorite>()))
            .Returns(Task.CompletedTask);

        _favoriteRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddFavoriteAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        _favoriteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Favorite>()), Times.Once);
        _favoriteRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddFavoriteAsync_AlreadyFavorited_ReturnsSuccessWithoutAdding()
    {
        // Arrange
        var existingFavorite = new Favorite { Id = 1, UserId = 1, RecipeId = 10 };

        _favoriteRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Expression<Func<Favorite, object>>[]>()))
            .ReturnsAsync([existingFavorite]);

        // Act
        var result = await _service.AddFavoriteAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        _favoriteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Favorite>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ExistingFavorite_RemovesAndReturnsSuccess()
    {
        // Arrange
        var existingFavorite = new Favorite { Id = 1, UserId = 1, RecipeId = 10 };

        _favoriteRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Expression<Func<Favorite, object>>[]>()))
            .ReturnsAsync([existingFavorite]);

        _favoriteRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RemoveFavoriteAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        _favoriteRepositoryMock.Verify(r => r.Remove(existingFavorite), Times.Once);
        _favoriteRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_NotFavorited_ReturnsFailure()
    {
        // Arrange
        _favoriteRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Expression<Func<Favorite, object>>[]>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.RemoveFavoriteAsync(1, 10);

        // Assert
        Assert.False(result.IsSuccess);
        _favoriteRepositoryMock.Verify(r => r.Remove(It.IsAny<Favorite>()), Times.Never);
    }

    [Fact]
    public async Task GetFavoritesByUserAsync_WithFavorites_ReturnsMappedRecipeDtos()
    {
        // Arrange
        var category = new Category { Id = 2, Name = "Desserts" };
        var author = new ApplicationUser { Id = 5, UserName = "chef_alice" };

        var recipe1 = new Recipe
        {
            Id = 10,
            Title = "Chocolate Cake",
            Description = "Rich chocolate cake",
            PrepTimeMin = 45,
            CategoryId = 2,
            AuthorId = 5,
            Category = category,
            Author = author
        };

        var recipe2 = new Recipe
        {
            Id = 11,
            Title = "Vanilla Tart",
            Description = "Sweet vanilla tart",
            PrepTimeMin = 30,
            CategoryId = 2,
            AuthorId = 5,
            Category = category,
            Author = author
        };

        var favorites = new List<Favorite>
        {
            new() { Id = 1, UserId = 1, RecipeId = 10, Recipe = recipe1 },
            new() { Id = 2, UserId = 1, RecipeId = 11, Recipe = recipe2 }
        };

        _favoriteRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Expression<Func<Favorite, object>>[]>()))
            .ReturnsAsync(favorites);

        // Act
        var result = await _service.GetFavoritesByUserAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        var dtos = result.Value!.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.All(dtos, dto =>
        {
            Assert.Equal("Desserts", dto.CategoryName);
            Assert.Equal("chef_alice", dto.AuthorName);
        });
    }

    [Fact]
    public async Task IsFavoriteAsync_WhenFavorited_ReturnsTrue()
    {
        // Arrange
        var existingFavorite = new Favorite { Id = 1, UserId = 1, RecipeId = 10 };

        _favoriteRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Expression<Func<Favorite, object>>[]>()))
            .ReturnsAsync([existingFavorite]);

        // Act
        var result = await _service.IsFavoriteAsync(1, 10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFavoriteAsync_WhenNotFavorited_ReturnsFalse()
    {
        // Arrange
        _favoriteRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Expression<Func<Favorite, object>>[]>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.IsFavoriteAsync(1, 10);

        // Assert
        Assert.False(result);
    }
}
