using KonFwEventBus;

namespace KonFwRabbitMq
{
    public class RabbitMqConfiguration : EventBusConfiguration
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ExchangeName { get; set; } = "konfw_event_bus";
        public string ExchangeType { get; set; } = "topic";
        public string QueueName { get; set; } = "konfw_event_queue";
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;
        public int PrefetchCount { get; set; } = 1;
        public bool PersistentDelivery { get; set; } = true;
    }
}
