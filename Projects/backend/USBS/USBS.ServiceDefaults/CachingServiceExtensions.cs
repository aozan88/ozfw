using OzFwRedisCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USBS.Components.Caching;

namespace USBS.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class CachingServiceExtensions
{
    public static IServiceCollection AddRedisCacheDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new RedisCacheConfiguration();
        var redisConfigSection = configuration.GetSection("Redis");

        services.AddRedisProvider(config =>
        {
            redisConfigSection.Bind(config);
        });
        return services;
    }
}
