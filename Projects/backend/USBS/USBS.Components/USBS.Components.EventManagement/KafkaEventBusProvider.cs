using OzFwKafka;
namespace USBS.Components.EventManagement
{

    public class KafkaEventBusProvider : KafkaEventBus
    {
        public KafkaEventBusProvider(KafkaConfiguration config) : base(config)
        {
        }

        public override async Task PublishAsync<TEvent>(TEvent @event)
        {
            Console.WriteLine("[Custom Logic] Preparing to publish...");
            await base.PublishAsync(@event); // Call base implementation
        }

        public override Task SubscribeAsync<TEvent, THandler>()
        {
            Console.WriteLine("[Custom Logic] Preparing to subscribe...");
            return base.SubscribeAsync<TEvent, THandler>();
        }

        public override Task UnsubscribeAsync<TEvent, THandler>()
        {
            Console.WriteLine("[Custom Logic] Cleaning up subscription...");
            return base.UnsubscribeAsync<TEvent, THandler>();
        }
    }
}
