using KonFwEventBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ortak.Application.Contract;
using Ortak.Data.Repositories;
using USBS.Components.Caching;
using USBS.Components.EventManagement;
using USBS.Ortak.API.Consumers;

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

       
        services.AddScoped<IEventHandler<SistemTanimTipiSilindiEvent>, SistemTanimTipiSilindiEventHandler>();
        services.AddScoped<ISistemTanimRepository, SistemTanimRepository>();
        services.AddScoped<SistemTanimTipiSilindiEventHandler>();

        return services;
    }

    public static async Task UseEventBusSubscriptions(this IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
        await eventBus.SubscribeAsync<SistemTanimTipiSilindiEvent, SistemTanimTipiSilindiEventHandler>();

        //using (var scope = app.ApplicationServices.CreateScope())
        //{
        //    var eventBusStarter = scope.ServiceProvider.GetRequiredService<RabbitMqEventBusStarter>();
        //    await eventBusStarter.StartAsync();
        //}
    }
}
