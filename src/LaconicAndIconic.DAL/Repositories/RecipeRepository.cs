using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.DAL.Repositories;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IEnumerable<Recipe> Recipes, int TotalCount)> SearchAsync(
        string? searchTerm,
        int? categoryId,
        string? sortBy,
        int pageNumber,
        int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var query = Context.Set<Recipe>()
            .Include(r => r.Category)
            .Include(r => r.Author)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchWords = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in searchWords)
            {
                var pattern = $"%{word}%";
                query = query.Where(r =>
                    EF.Functions.ILike(r.Title, pattern) ||
                    EF.Functions.ILike(r.Category.Name, pattern) ||
                    (r.Description != null && EF.Functions.ILike(r.Description, pattern)));
            }
        }

        query = sortBy switch
        {
            "title_asc" => query.OrderBy(r => r.Title),
            "title_desc" => query.OrderByDescending(r => r.Title),
            "prepTime_asc" => query.OrderBy(r => r.PrepTimeMin),
            "prepTime_desc" => query.OrderByDescending(r => r.PrepTimeMin),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var recipes = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (recipes, totalCount);
    }
}
