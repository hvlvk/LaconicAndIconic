using System.Linq;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;


namespace LaconicAndIconic.BLL.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IRecipeRepository _recipeRepository;

    public ReportService(IReportRepository reportRepository, IRecipeRepository recipeRepository)
    {
        _reportRepository = reportRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<Result> CreateAsync(CreateReportDto dto, int reporterId)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return Result.Failure("Вкажіть причину скарги");
        }

        var recipes = await _recipeRepository.FindAsync(r => r.Id == dto.RecipeId);
        var recipe = recipes.FirstOrDefault();
        if (recipe == null)
        {
            return Result.Failure("Рецепт не знайдено");
        }

        if (recipe.AuthorId != dto.ReportedUserId)
        {
            return Result.Failure("Автор рецепту не відповідає скарзі");
        }

        var reportExists = await _reportRepository.AnyAsync(r => r.RecipeId == dto.RecipeId && r.ReporterId == reporterId);
        if (reportExists)
        {
            return Result.Failure("Ви вже надсилали скаргу на цей рецепт");
        }

        var report = new Report
        {
            RecipeId = dto.RecipeId,
            ReporterId = reporterId,
            ReportedUserId = dto.ReportedUserId,
            Reason = dto.Reason
        };

        await _reportRepository.AddAsync(report);
        await _reportRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<ReportDto>>> GetAllAsync()
    {
        var reportList = await _reportRepository.GetAllWithUsersAsync();
        
        var reports = reportList.Select(r => new ReportDto
        {
            Id = r.Id,
            RecipeId = r.RecipeId,
            RecipeTitle = r.Recipe.Title,
            ReporterId = r.ReporterId,
            ReporterName = r.Reporter?.UserName ?? string.Empty,
            ReportedUserId = r.ReportedUserId,
            ReportedUserName = r.ReportedUser?.UserName ?? string.Empty,
            Reason = r.Reason,
            CreatedAt = r.CreatedAt
        }).ToList();

        return reports;
    }

    public async Task<Result<ReportDto>> GetByIdAsync(int reportId)
    {
        var report = await _reportRepository.GetByIdWithUsersAsync(reportId);

        if (report == null)
        {
            return "Скаргу не знайдено";
        }

        return new ReportDto
        {
            Id = report.Id,
            RecipeId = report.RecipeId,
            RecipeTitle = report.Recipe.Title,
            ReporterId = report.ReporterId,
            ReporterName = report.Reporter.UserName ?? string.Empty,
            ReportedUserId = report.ReportedUserId,
            ReportedUserName = report.ReportedUser.UserName ?? string.Empty,
            Reason = report.Reason,
            CreatedAt = report.CreatedAt
        };
    }

    public async Task<Result> DismissAsync(int reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            return Result.Failure("Скаргу не знайдено");
        }

        _reportRepository.Remove(report);
        await _reportRepository.SaveChangesAsync();

        return Result.Success();
    }
}
