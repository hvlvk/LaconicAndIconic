namespace LaconicAndIconic.Web.Models;

public sealed class TheMealDbOptions
{
    public const string SectionName = "ExternalApis:TheMealDb";

    public Uri BaseUrl { get; set; } = new("https://www.themealdb.com/api/json/v1/1/");

    public int TimeoutSeconds { get; set; } = 10;

    public int CacheMinutes { get; set; } = 30;

    public int DefaultCategoryId { get; set; } = 3;

    public int DefaultPrepTimeMinutes { get; set; } = 30;

    public int DefaultServings { get; set; } = 1;
}
