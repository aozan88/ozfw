using OzFwCaching;
using OzFwRedisCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USBS.Components.Caching;

namespace USBS.ServiceDefaults;

/// <summary>Önbellek kaydı. Servisler IKonCache'e bağlıdır; altyapı yapılandırmadan seçilir.</summary>
public static class CachingServiceExtensions
{
    public const string ProviderKey = "Cache:Provider";

    /// <summary>
    /// Cache:Provider = memory (varsayılan) | redis. redis → "Redis" bölümünden ayar okur.
    /// </summary>
    public static IServiceCollection AddOzFwCache(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = (configuration[ProviderKey] ?? "memory").Trim().ToLowerInvariant();
        switch (provider)
        {
            case "memory":
                var config = new CacheConfiguration();
                configuration.GetSection("Cache").Bind(config);
                services.AddSingleton<IKonCache>(new MemoryKonCache(config));
                return services;
            case "redis":
                return services.AddRedisCacheDefaults(configuration);
            default:
                throw new InvalidOperationException($"Bilinmeyen {ProviderKey} değeri: '{provider}'. Geçerli değerler: memory, redis.");
        }
    }

    public static IServiceCollection AddRedisCacheDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfigSection = configuration.GetSection("Redis");

        services.AddRedisProvider(config =>
        {
            redisConfigSection.Bind(config);
        });
        return services;
    }
}
