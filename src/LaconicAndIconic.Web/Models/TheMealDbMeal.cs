using System.Text.Json.Serialization;

namespace LaconicAndIconic.Web.Models;

public sealed record TheMealDbMeal(
    [property: JsonPropertyName("idMeal")] string IdMeal,
    [property: JsonPropertyName("strMeal")] string StrMeal,
    [property: JsonPropertyName("strCategory")] string? StrCategory,
    [property: JsonPropertyName("strArea")] string? StrArea,
    [property: JsonPropertyName("strInstructions")] string? StrInstructions,
    [property: JsonPropertyName("strMealThumb")] string? StrMealThumb,
    [property: JsonPropertyName("strSource")] string? StrSource)
{
    [JsonExtensionData]
    public IDictionary<string, object?> AdditionalFields { get; init; } = new Dictionary<string, object?>();
}
