using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LaconicAndIconic.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<IExampleService, ExampleService>();

        return services;
    }
}
