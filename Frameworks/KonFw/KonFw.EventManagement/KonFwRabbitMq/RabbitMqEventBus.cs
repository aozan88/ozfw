using KonFwEventBus;
using System.Text.Json;

namespace KonFwRabbitMq
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly RabbitMqConfiguration _config;
        private readonly Dictionary<Type, List<Type>> _handlers = new Dictionary<Type, List<Type>>();

        public RabbitMqEventBus(RabbitMqConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (!_config.IsEnabled) return;

            string routingKey = GetRoutingKey<TEvent>();
            string serializedEvent = JsonSerializer.Serialize(@event);

            Console.WriteLine($"[RabbitMQ Publish] {_config.HostName}:{_config.Port} - " +
                             $"Exchange: {_config.ExchangeName} - RoutingKey: {routingKey} - EventId: {@event.Id}");

            // In a real implementation, we'd use RabbitMQ.Client to publish the event
        }

        public Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            if (!_config.IsEnabled) return Task.CompletedTask;

            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Type>();
            }

            if (_handlers[eventType].Any(h => h == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventType.Name}'");
            }

            _handlers[eventType].Add(handlerType);

            string routingKey = GetRoutingKey<TEvent>();
            Console.WriteLine($"[RabbitMQ Subscribe] {_config.HostName}:{_config.Port} - " +
                             $"Exchange: {_config.ExchangeName} - RoutingKey: {routingKey} - Handler: {handlerType.Name}");

            // In a real implementation, we'd use RabbitMQ.Client to bind to the queue

            return Task.CompletedTask;
        }
        public Task UnsubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            if (!_config.IsEnabled) return Task.CompletedTask;

            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            if (!_handlers.ContainsKey(eventType))
            {
                return Task.CompletedTask;
            }

            _handlers[eventType].Remove(handlerType);

            string routingKey = GetRoutingKey<TEvent>();
            Console.WriteLine($"[RabbitMQ Unsubscribe] {_config.HostName}:{_config.Port} - " +
                             $"Exchange: {_config.ExchangeName} - RoutingKey: {routingKey} - Handler: {handlerType.Name}");
            return Task.CompletedTask;
        }

        private string GetRoutingKey<TEvent>()
        {
            return $"{_config.ExchangeType}{typeof(TEvent).Name}";
        }
    }
}