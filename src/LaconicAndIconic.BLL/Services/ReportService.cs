using System.Linq;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;

namespace LaconicAndIconic.BLL.Services;

public class ReportService : IReportService
{
    private readonly IRepository<Report> _reportRepository;
    private readonly IRecipeRepository _recipeRepository;

    public ReportService(IRepository<Report> reportRepository, IRecipeRepository recipeRepository)
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
}
