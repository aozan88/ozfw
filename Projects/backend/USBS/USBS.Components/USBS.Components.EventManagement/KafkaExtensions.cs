using Microsoft.Extensions.DependencyInjection;
using KonFwKafka;
using KonFwEventBus;
namespace USBS.Components.EventManagement
{

    public static class KafkaEventBusExtensions
    {
        public static IServiceCollection AddKafkaEventBusProvider(this IServiceCollection services, Action<KafkaConfiguration> configure)
        {
            var config = new KafkaConfiguration();
            configure(config);

            services.AddSingleton(config);
            services.AddSingleton<IEventBus>(sp => new KafkaEventBusProvider(config));

            return services;
        }
    }
}
