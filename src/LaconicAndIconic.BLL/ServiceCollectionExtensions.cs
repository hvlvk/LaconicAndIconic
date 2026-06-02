using LaconicAndIconic.BLL.Decorators;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.BLL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISharedListService, SharedListService>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

        services.AddScoped<RecipeService>();

        services.AddScoped<IRecipeService>(provider =>
        {
            var innerService = provider.GetRequiredService<RecipeService>();
            var memoryCache = provider.GetRequiredService<IMemoryCache>();
            var invalidationService = provider.GetRequiredService<ICacheInvalidationService>();
            var options = provider.GetRequiredService<IOptions<CachingOptions>>();

            return new CachedRecipeService(
                innerService,
                memoryCache,
                invalidationService,
                options);
        });

        return services;
    }
}
