using System.Text.Json.Serialization;

namespace LaconicAndIconic.Web.Models;

public sealed record TheMealDbSearchResponse(
    [property: JsonPropertyName("meals")] IReadOnlyCollection<TheMealDbMeal>? Meals);
