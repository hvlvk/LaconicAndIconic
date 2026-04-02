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
        var dbQuery = Context.Set<Recipe>()
            .Include(r => r.Category)
            .Include(r => r.Author)
            .AsNoTracking();

        if (filter.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.CategoryId == filter.CategoryId.Value);
        }

        var candidates = await dbQuery.ToListAsync();

        IEnumerable<Recipe> filteredResults = candidates;
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchWords = filter.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            filteredResults = filteredResults.Where(r =>
                searchWords.All(word =>
                    r.Title.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    r.Category.Name.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(word, StringComparison.OrdinalIgnoreCase)));
        }

        var sortedResults = ApplySorting(filteredResults, filter.SortBy).ToList();

        var totalCount = sortedResults.Count;
        var pagedRecipes = sortedResults
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new RecipeSearchResult
        {
            Recipes = pagedRecipes,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            SearchTerm = filter.SearchTerm,
            CategoryId = filter.CategoryId,
            SortBy = filter.SortBy
        };
    }

    private static IEnumerable<Recipe> ApplySorting(IEnumerable<Recipe> query, string? sortBy)
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
