using Microsoft.Extensions.DependencyInjection;
using KonFwEventBus;
using KonFwRabbitMq;
namespace USBS.Components.Caching
{
    public static class RabbitMqExtensions
    {
        public static IServiceCollection AddRabbitMqEventBusProvider(this IServiceCollection services, Action<RabbitMqConfiguration> configure)
        {
            var config = new RabbitMqConfiguration();
            configure(config);

            services.AddSingleton(config);
            services.AddSingleton<IEventBus, RabbitMqEventBus>();

            return services;
        }
    }
}
