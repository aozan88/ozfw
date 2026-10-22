using OzFwEventBus;
using System.Text.Json;

namespace OzFwKafka
{
    public class KafkaEventBus : IEventBus
    {
        private readonly KafkaConfiguration _config;

        public KafkaEventBus(KafkaConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public virtual Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            Console.WriteLine($"Published to Kafka: {typeof(TEvent).Name} with config {_config.BootstrapServers}");
            return Task.CompletedTask; // Mocked
        }

        public virtual Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent where THandler : IEventHandler<TEvent>
        {
            Console.WriteLine($"Subscribed to Kafka: {typeof(TEvent).Name}");
            return Task.CompletedTask; // Mocked
        }

        public virtual Task UnsubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent where THandler : IEventHandler<TEvent>
        {
            Console.WriteLine($"Unsubscribed from Kafka: {typeof(TEvent).Name}");
            return Task.CompletedTask; // Mocked
        }
    }
}
