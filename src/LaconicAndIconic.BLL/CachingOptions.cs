namespace LaconicAndIconic.BLL;

/// <summary>
/// Configuration options for caching behavior.
/// </summary>
public class CachingOptions
{
    /// <summary>
    /// Gets or sets the cache lifetime for categories in minutes.
    /// Default: 60 minutes
    /// </summary>
    public int CategoriesCacheLifetimeMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the cache lifetime for recipes in minutes.
    /// Default: 15 minutes
    /// </summary>
    public int RecipesCacheLifetimeMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the cache lifetime for users in minutes.
    /// Default: 30 minutes
    /// </summary>
    public int UsersCacheLifetimeMinutes { get; set; } = 30;
}
