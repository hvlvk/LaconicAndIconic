namespace LaconicAndIconic.BLL;

public class CachingOptions
{
    public int CategoriesCacheLifetimeMinutes { get; set; } = 60;

    public int RecipesCacheLifetimeMinutes { get; set; } = 15;

    public int UsersCacheLifetimeMinutes { get; set; } = 30;
}
