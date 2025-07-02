using KRTBanking.Application.Interfaces.Services;
using KRTBanking.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KRTBanking.Application.Extensions;

/// <summary>
/// Extension methods for configuring Application layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
