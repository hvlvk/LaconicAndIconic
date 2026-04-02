using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Repositories;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<RecipeSearchResult> SearchAsync(RecipeSearchFilter filter)
    {
        var dbQuery = Context.Set<Recipe>().AsNoTracking();

        if (filter.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.CategoryId == filter.CategoryId.Value);
        }

        var candidates = await dbQuery.Select(r => new RecipeSearchCandidate
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            CategoryName = r.Category.Name,
            CreatedAt = r.CreatedAt,
            PrepTimeMin = r.PrepTimeMin
        }).ToListAsync();

        IEnumerable<RecipeSearchCandidate> filteredResults = candidates;
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchWords = filter.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            filteredResults = filteredResults.Where(c =>
                searchWords.All(word =>
                    c.Title.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    c.CategoryName.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description?.Contains(word, StringComparison.OrdinalIgnoreCase) ?? false)));
        }

        var sortedResults = ApplySorting(filteredResults, filter.SortBy).ToList();

        var totalCount = sortedResults.Count;
        var pagedIds = sortedResults
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => c.Id)
            .ToList();

        var recipes = await Context.Set<Recipe>()
            .Include(r => r.Category)
            .Include(r => r.Author)
            .Where(r => pagedIds.Contains(r.Id))
            .AsNoTracking()
            .ToListAsync();

        var finalRecipes = recipes
            .OrderBy(r => pagedIds.IndexOf(r.Id))
            .ToList();

        return new RecipeSearchResult
        {
            Recipes = finalRecipes,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            SearchTerm = filter.SearchTerm,
            CategoryId = filter.CategoryId,
            SortBy = filter.SortBy
        };
    }

    private sealed class RecipeSearchCandidate
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PrepTimeMin { get; set; }
    }

    private static IEnumerable<RecipeSearchCandidate> ApplySorting(IEnumerable<RecipeSearchCandidate> query, string? sortBy)
    {
        return sortBy switch
        {
            "title_asc" => query.OrderBy(r => r.Title),
            "title_desc" => query.OrderByDescending(r => r.Title),
            "prepTime_asc" => query.OrderBy(r => r.PrepTimeMin),
            "prepTime_desc" => query.OrderByDescending(r => r.PrepTimeMin),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };
    }
}
