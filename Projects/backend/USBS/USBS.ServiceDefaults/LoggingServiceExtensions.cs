using Common.Logging;
using KonFwELK;
using KonFwGraylog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using USBS.Components.Logging;

namespace USBS.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
using KonFwLogging; // Make sure this namespace is included

public static class LoggingServiceExtensions
{
    /// <summary>
    /// Adds complete logging infrastructure including provider selection based on configuration
    /// </summary>
    public static IServiceCollection AddLoggingDefaults(WebApplicationBuilder builder)
    {
        // Configure Serilog
        builder.Host.UseSerilog(SeriLogger.Configure);

        // Get provider configuration from settings
        var loggingProvider = builder.Configuration.GetValue<string>("Logging:Provider") ?? "Graylog";

        // Configure the provider based on configuration
        switch (loggingProvider.ToLowerInvariant())
        {
            case "elk":
                builder.Services.AddELKLogging(builder.Configuration);
                break;
            case "graylog":
            default:
                builder.Services.AddGraylogLogging(builder.Configuration);
                break;
        }

        // Add LoggingDelegatingHandler with custom named client
        builder.Services.AddTransient<LoggingDelegatingHandler>();
        builder.Services.AddHttpClient("LoggingClient")
            .AddHttpMessageHandler<LoggingDelegatingHandler>();

        return builder.Services;
    }

    /// <summary>
    /// Adds Graylog logging using configuration
    /// </summary>
    public static IServiceCollection AddGraylogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        var graylogConfig = new GraylogConfiguration();
        configuration.GetSection("ElasticConfiguration").Bind(graylogConfig);

        // Register the configuration
        services.AddSingleton(graylogConfig);

        // Register the Graylog logger provider
        services.AddSingleton<GraylogLoggerProvider>();

        // Register IKonLogger implementation explicitly
        services.AddSingleton<IKonLogger>(sp => sp.GetRequiredService<GraylogLoggerProvider>());

        return services;
    }

    /// <summary>
    /// Adds ELK logging using configuration
    /// </summary>
    public static IServiceCollection AddELKLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        var elkConfig = new ELKConfiguration();
        configuration.GetSection("ElasticConfiguration").Bind(elkConfig);

        // Register the configuration
        services.AddSingleton(elkConfig);

        // Register the ELK logger provider
        services.AddSingleton<ELKLoggerProvider>();

        // Register IKonLogger implementation explicitly
        services.AddSingleton<IKonLogger>(sp => sp.GetRequiredService<ELKLoggerProvider>());

        return services;
    }
}