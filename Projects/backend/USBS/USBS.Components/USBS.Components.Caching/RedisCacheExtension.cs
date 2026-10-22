using OzFwCaching;
using OzFwRedisCaching;
using Microsoft.Extensions.DependencyInjection;

namespace USBS.Components.Caching
{
    public static class RedisCacheExtension
    {
        public static IServiceCollection AddRedisProvider(this IServiceCollection services, Action<RedisCacheConfiguration> configure)
        {
            var config = new RedisCacheConfiguration();
            configure(config);

            services.AddSingleton(config);
            services.AddSingleton(sp => new RedisCacheProvider(config));

            return services;
        }
    }
}
