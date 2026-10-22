using OzFwEventBus;

namespace OzFwKafka
{
    public class KafkaConfiguration : EventBusConfiguration
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "ozfw-consumer";
        public string ClientId { get; set; } = "ozfw-producer";
        public bool EnableAutoCommit { get; set; } = false;
        public int SessionTimeoutMs { get; set; } = 10000;
        public string DefaultTopicPrefix { get; set; } = "ozfw-";
        public int MaxRetryAttempts { get; set; } = 5;
        public bool EnableSsl { get; set; } = false;
        public string SaslUsername { get; set; }
        public string SaslPassword { get; set; }
        public string SaslMechanism { get; set; } = "PLAIN";
        public string SecurityProtocol { get; set; } = "SASL_SSL";
    }
}
