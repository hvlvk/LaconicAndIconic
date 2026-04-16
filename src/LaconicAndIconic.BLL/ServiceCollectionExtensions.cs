using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LaconicAndIconic.BLL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICommentService, CommentService>();

        return services;
    }
}
