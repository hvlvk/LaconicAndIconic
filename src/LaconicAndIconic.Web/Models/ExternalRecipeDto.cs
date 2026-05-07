namespace LaconicAndIconic.Web.Models;

public sealed record ExternalRecipeDto(
    string ExternalId,
    string Title,
    string? Category,
    string? Area,
    string? Instructions,
    string Ingredients,
    Uri? ThumbnailUri,
    Uri? SourceUri);
