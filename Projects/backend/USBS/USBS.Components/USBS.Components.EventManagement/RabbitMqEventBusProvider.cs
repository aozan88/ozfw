using OzFwRabbitMq;

namespace USBS.Components.EventManagement
{
    public class RabbitMqEventBusProvider : RabbitMqEventBus
    {
        public RabbitMqEventBusProvider(RabbitMqConfiguration config, IServiceProvider serviceProvider)
            : base(config, serviceProvider)
        {
        }

        public override async Task PublishAsync<TEvent>(TEvent @event)
        {
            Console.WriteLine($"[RabbitMqEventBusProvider] Publishing event: {typeof(TEvent).Name}");
            await base.PublishAsync(@event);
        }

        public override async Task SubscribeAsync<TEvent, THandler>()
        {
            Console.WriteLine($"[RabbitMqEventBusProvider] Subscribing to event: {typeof(TEvent).Name}");
            await base.SubscribeAsync<TEvent, THandler>();
        }

        public override Task UnsubscribeAsync<TEvent, THandler>()
        {
            Console.WriteLine($"[RabbitMqEventBusProvider] Unsubscribing from event: {typeof(TEvent).Name}");
            return base.UnsubscribeAsync<TEvent, THandler>();
        }
    }
}