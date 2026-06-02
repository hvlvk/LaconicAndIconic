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
        var query = Context.Set<Recipe>()
            .Include(r => r.Category)
            .Include(r => r.Author)
            .Include(r => r.Ratings)
            .AsNoTracking()
            .AsSplitQuery();

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchWords = filter.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in searchWords)
            {
                var pattern = $"%{EscapeLikePattern(word)}%";

                query = query.Where(r =>
                    EF.Functions.ILike(r.Title, pattern) ||
                    EF.Functions.ILike(r.Category.Name, pattern) ||
                    EF.Functions.ILike(r.Description, pattern));
            }
        }

        query = ApplySorting(query, filter.SortBy);

        var totalCount = await query.CountAsync();

        var pagedRecipes = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

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

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }

    private static IQueryable<Recipe> ApplySorting(IQueryable<Recipe> query, string? sortBy)
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
