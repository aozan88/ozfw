using KonFwEventBus;
using KonFwRabbitMq;

namespace USBS.Components.EventManagement
{
    public class RabbitMqEventBusProvider : RabbitMqEventBus
    {
        private readonly RabbitMqConfiguration _config;

        public RabbitMqEventBusProvider(RabbitMqConfiguration config) : base(config)
        {
            _config = config;
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            Console.WriteLine($"Published to RabbitMq: {typeof(TEvent).Name} with config {_config.QueueName}");
            return Task.CompletedTask; // Mocked
        }

        public Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent where THandler : IEventHandler<TEvent>
        {
            Console.WriteLine($"Subscribed to RabbitMq: {typeof(TEvent).Name}");
            return Task.CompletedTask; // Mocked
        }

        public Task UnsubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent where THandler : IEventHandler<TEvent>
        {
            Console.WriteLine($"Unsubscribed from RabbitMq: {typeof(TEvent).Name}");
            return Task.CompletedTask; // Mocked
        }
    }
}
