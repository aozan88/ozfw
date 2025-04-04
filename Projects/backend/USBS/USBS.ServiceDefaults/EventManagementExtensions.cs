using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USBS.Components.Caching;
using USBS.Components.EventManagement;

namespace USBS.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class EventManagementExtensions
{
    public static IServiceCollection AddKafkaEventBusDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaConfigSection = configuration.GetSection("KafkaEventBus");

        services.AddKafkaEventBusProvider(config =>
        {
            kafkaConfigSection.Bind(config);
        });

        return services;
    }

    public static IServiceCollection AddRabbitMqEventBusDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        /*
        var rabbitMqConfigSection = configuration.GetSection("RabbitMqEventBus");
        services.AddRabbitMqEventBusDefaults(rabbitMqConfigSection);
        return services;
        */

        var rabbitMqConfigSection = configuration.GetSection("RabbitMqEventBus");
        services.AddRabbitMqEventBusProvider(config =>
        {
            rabbitMqConfigSection.Bind(config);
        });
        //services.AddSingleton(config);
        //services.AddSingleton<IEventBus>(sp => new RabbitMqEventBusProvider(config));

        return services;
        

    }
}
