using KonFwELK;
using KonFwGraylog;
using KonFwLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USBS.Components.Logging;

namespace USBS.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class LoggingServiceExtensions
{
    public static IServiceCollection AddGraylogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection("Graylog").Get<GraylogConfiguration>();
        services.AddSingleton(config);
        services.AddSingleton<IKonLogger, GraylogLoggerProvider>();
        services.AddSingleton<IKonLogger>(sp => sp.GetRequiredService<GraylogLoggerProvider>());
        return services;
    }

    public static IServiceCollection AddELKLogging(this IServiceCollection services, Action<ELKConfiguration> configAction = null)
    {
        var config = new ELKConfiguration();
        configAction?.Invoke(config);

        services.AddSingleton(config);
        services.AddSingleton<ELKLoggerProvider>();
        services.AddSingleton<IKonLogger>(sp => sp.GetRequiredService<ELKLoggerProvider>());

        return services;
    }
}
