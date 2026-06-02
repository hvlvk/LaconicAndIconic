using LaconicAndIconic.DAL.Entities;

namespace LaconicAndIconic.DAL.Interfaces;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<RecipeSearchResult> SearchAsync(RecipeSearchFilter filter);
}

