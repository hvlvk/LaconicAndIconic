using System.Linq.Expressions;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using MockQueryable.Moq;
using Moq;

namespace LaconicAndIconic.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IRepository<Report>> _reportRepositoryMock;
    private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _reportRepositoryMock = new Mock<IRepository<Report>>();
        _recipeRepositoryMock = new Mock<IRecipeRepository>();
        _service = new ReportService(_reportRepositoryMock.Object, _recipeRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidReport_AddsReport()
    {
        var dto = new CreateReportDto
        {
            RecipeId = 10,
            ReportedUserId = 3,
            Reason = "Неприйнятний рецепт"
        };

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>() ))
            .ReturnsAsync([new Recipe { Id = 10, AuthorId = 3 }]);
        _reportRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Report, bool>>>()))
            .ReturnsAsync(false);
        _reportRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto, reporterId: 7);

        Assert.True(result.IsSuccess);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.Is<Report>(report =>
            report.RecipeId == 10 &&
            report.ReporterId == 7 &&
            report.ReportedUserId == 3 &&
            report.Reason == "Неприйнятний рецепт")), Times.Once);
        _reportRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyReason_ReturnsFailure()
    {
        var dto = new CreateReportDto { RecipeId = 1, ReportedUserId = 2, Reason = "  " };

        var result = await _service.CreateAsync(dto, reporterId: 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("Вкажіть причину скарги", result.ErrorMessage);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RecipeNotFound_ReturnsFailure()
    {
        var dto = new CreateReportDto { RecipeId = 404, ReportedUserId = 2, Reason = "Спам" };
        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>() ))
            .ReturnsAsync([]);

        var result = await _service.CreateAsync(dto, reporterId: 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("Рецепт не знайдено", result.ErrorMessage);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ReportedUserDoesNotMatchAuthor_ReturnsFailure()
    {
        var dto = new CreateReportDto { RecipeId = 10, ReportedUserId = 99, Reason = "Спам" };
        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>() ))
            .ReturnsAsync([new Recipe { Id = 10, AuthorId = 3 }]);

        var result = await _service.CreateAsync(dto, reporterId: 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("Автор рецепту не відповідає скарзі", result.ErrorMessage);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateReport_ReturnsFailure()
    {
        var dto = new CreateReportDto { RecipeId = 10, ReportedUserId = 3, Reason = "Спам" };
        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>() ))
            .ReturnsAsync([new Recipe { Id = 10, AuthorId = 3 }]);
        _reportRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Report, bool>>>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(dto, reporterId: 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ви вже надсилали скаргу на цей рецепт", result.ErrorMessage);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsReportsOrderedByNewest()
    {
        var olderReport = CreateReport(1, DateTime.UtcNow.AddHours(-2), "Older");
        var newerReport = CreateReport(2, DateTime.UtcNow, "Newer");
        var mockDbSet = new List<Report> { olderReport, newerReport }
            .AsQueryable()
            .BuildMockDbSet();

        _reportRepositoryMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        var result = await _service.GetAllAsync();

        Assert.True(result.IsSuccess);
        var reports = Assert.IsAssignableFrom<IEnumerable<ReportDto>>(result.Value).ToList();
        Assert.Equal(2, reports.Count);
        Assert.Equal(2, reports[0].Id);
        Assert.Equal("Newer", reports[0].RecipeTitle);
        Assert.Equal("reporter_2", reports[0].ReporterName);
        Assert.Equal("author_2", reports[0].ReportedUserName);
    }

    [Fact]
    public async Task GetByIdAsync_ReportExists_ReturnsMappedReport()
    {
        var report = CreateReport(8, DateTime.UtcNow, "Borscht");
        var mockDbSet = new List<Report> { report }.AsQueryable().BuildMockDbSet();
        _reportRepositoryMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        var result = await _service.GetByIdAsync(8);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(8, result.Value!.Id);
        Assert.Equal("Borscht", result.Value.RecipeTitle);
        Assert.Equal(report.RecipeId, result.Value.RecipeId);
        Assert.Equal(report.ReportedUserId, result.Value.ReportedUserId);
    }

    [Fact]
    public async Task GetByIdAsync_ReportMissing_ReturnsFailure()
    {
        var mockDbSet = Array.Empty<Report>().AsQueryable().BuildMockDbSet();
        _reportRepositoryMock.Setup(r => r.GetQueryable()).Returns(mockDbSet.Object);

        var result = await _service.GetByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal("Скаргу не знайдено", result.ErrorMessage);
    }

    [Fact]
    public async Task DismissAsync_ReportExists_RemovesReport()
    {
        var report = CreateReport(4, DateTime.UtcNow, "Cake");
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(report);

        var result = await _service.DismissAsync(4);

        Assert.True(result.IsSuccess);
        _reportRepositoryMock.Verify(r => r.Remove(report), Times.Once);
        _reportRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DismissAsync_ReportMissing_ReturnsFailure()
    {
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((Report?)null);

        var result = await _service.DismissAsync(404);

        Assert.False(result.IsSuccess);
        Assert.Equal("Скаргу не знайдено", result.ErrorMessage);
        _reportRepositoryMock.Verify(r => r.Remove(It.IsAny<Report>()), Times.Never);
    }

    private static Report CreateReport(int id, DateTime createdAt, string recipeTitle)
    {
        var reporter = new ApplicationUser { Id = id + 10, UserName = $"reporter_{id}" };
        var author = new ApplicationUser { Id = id + 20, UserName = $"author_{id}" };
        var recipe = new Recipe
        {
            Id = id + 100,
            Title = recipeTitle,
            AuthorId = author.Id,
            Author = author
        };

        return new Report
        {
            Id = id,
            RecipeId = recipe.Id,
            Recipe = recipe,
            ReporterId = reporter.Id,
            Reporter = reporter,
            ReportedUserId = author.Id,
            ReportedUser = author,
            Reason = $"Reason {id}",
            CreatedAt = createdAt
        };
    }
}
