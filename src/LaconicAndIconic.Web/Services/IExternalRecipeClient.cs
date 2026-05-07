using LaconicAndIconic.Web.Models;

namespace LaconicAndIconic.Web.Services;

public interface IExternalRecipeClient
{
    Task<IReadOnlyCollection<ExternalRecipeDto>> SearchRecipesAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    Task<ExternalRecipeDto?> GetRecipeByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default);
}
