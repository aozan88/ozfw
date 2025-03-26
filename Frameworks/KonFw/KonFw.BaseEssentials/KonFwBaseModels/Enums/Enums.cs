namespace KonFwBaseModels.Enums
{
    public enum CacheType
    {
        Memory,
        Redis,
        Distributed
    }

    public enum MessageBrokerType
    {
        RabbitMQ,
        Kafka,
        AzureServiceBus,
        InMemory
    }
}
