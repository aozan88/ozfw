using Microsoft.Extensions.DependencyInjection;
using OzFwEventBus;
using OzFwRabbitMq;
using Microsoft.Extensions.Configuration;
using USBS.Components.EventManagement;
namespace USBS.Components.Caching
{
    public static class RabbitMqExtensions
    {
        public static IServiceCollection AddRabbitMqEventBusProvider(this IServiceCollection services, Action<RabbitMqConfiguration> configure)
        {
            var config = new RabbitMqConfiguration();
            configure(config);
           
            services.AddSingleton(config);
            // Uygulamaya özgü genişletme noktası (Kafka tarafıyla aynı desen)
            services.AddSingleton<IEventBus, RabbitMqEventBusProvider>();

            return services;
        }
    }
}
