using OzFwEventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USBS.Components.Caching;
using USBS.Components.EventManagement;

namespace USBS.ServiceDefaults;

/// <summary>
/// Olay altyapısı kaydı. Servisler yalnızca IEventBus'a bağlıdır; hangi altyapının kullanılacağı yapılandırmadan seçilir.
/// Modüle özgü abonelikler (handler'lar) ilgili modülün kendi kayıtlarında yapılır.
/// </summary>
public static class EventManagementExtensions
{
    public const string ProviderKey = "EventBus:Provider";

    public static class Providers
    {
        public const string InMemory = "inmemory";
        public const string RabbitMq = "rabbitmq";
        public const string Kafka = "kafka";
        public const string None = "none";
    }

    /// <summary>
    /// EventBus:Provider = inmemory (varsayılan) | rabbitmq | kafka | none.
    /// rabbitmq → "RabbitMqEventBus", kafka → "KafkaEventBus" bölümünden ayar okur.
    /// </summary>
    public static IServiceCollection AddOzFwEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = (configuration[ProviderKey] ?? Providers.InMemory).Trim().ToLowerInvariant();
        return provider switch
        {
            Providers.InMemory => services.AddSingleton<IEventBus, InMemoryEventBus>(),
            Providers.RabbitMq => services.AddRabbitMqEventBusDefaults(configuration),
            Providers.Kafka => services.AddKafkaEventBusDefaults(configuration),
            Providers.None => services.AddSingleton<IEventBus, NullEventBus>(),
            _ => throw new InvalidOperationException(
                $"Bilinmeyen {ProviderKey} değeri: '{provider}'. Geçerli değerler: inmemory, rabbitmq, kafka, none.")
        };
    }

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
        var rabbitMqConfigSection = configuration.GetSection("RabbitMqEventBus");
        services.AddRabbitMqEventBusProvider(config =>
        {
            rabbitMqConfigSection.Bind(config);
        });

        return services;
    }
}
