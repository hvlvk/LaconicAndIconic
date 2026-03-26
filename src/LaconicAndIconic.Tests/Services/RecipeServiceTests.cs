using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace LaconicAndIconic.Tests.Services;

public class RecipeServiceTests
{
    private readonly Mock<IRepository<Recipe>> _recipeRepositoryMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _recipeRepositoryMock = new Mock<IRepository<Recipe>>();
        _fileServiceMock = new Mock<IFileService>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _service = new RecipeService(_recipeRepositoryMock.Object, _fileServiceMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateRecipeAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateRecipeDto
        {
            Title = "Test Recipe",
            Description = "A delicious test recipe",
            PrepTimeMin = 30,
            CategoryId = 1
        };

        _categoryRepositoryMock.Setup(repo => repo.ExistsAsync(1)).ReturnsAsync(true);
        _recipeRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Recipe>())).Returns(Task.CompletedTask);
        _recipeRepositoryMock.Setup(repo => repo.GetByIdAsync(0)).ReturnsAsync(new Recipe { Id = 1, Title = "Test Recipe", CategoryId = 1, AuthorId = 1, PrepTimeMin = 30, Description = "A delicious test recipe" });

        // Act
        var result = await _service.CreateRecipeAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test Recipe", result.Value.Title);
        _recipeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Recipe>()), Times.Once);
        _recipeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRecipeAsync_MissingTitle_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateRecipeDto { Title = "  " };

        // Act
        var result = await _service.CreateRecipeAsync(1, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Title is required", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateRecipeAsync_CategoryNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateRecipeDto { Title = "Title", CategoryId = 99 };
        _categoryRepositoryMock.Setup(repo => repo.ExistsAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _service.CreateRecipeAsync(1, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetRecipesByAuthorIdAsync_ReturnsRecipes()
    {
        // Arrange
        var authorId = 1;
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "R1", AuthorId = authorId, CreatedAt = DateTime.UtcNow },
            new Recipe { Id = 2, Title = "R2", AuthorId = authorId, CreatedAt = DateTime.UtcNow.AddMinutes(-10) }
        };

        _recipeRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync(recipes);

        // Act
        var result = await _service.GetRecipesByAuthorIdAsync(authorId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Equal("R1", result.Value!.First().Title);
    }
}