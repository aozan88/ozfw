namespace KonFwEventBus
{
    public class EventBusConfiguration
    {
        public bool IsEnabled { get; set; } = true;
        public int RetryCount { get; set; } = 3;
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(5);
        public bool EnableLogging { get; set; } = true;
    }
}
