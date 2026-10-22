namespace OzFwCaching
{
    public class CacheConfiguration
    {
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
        public bool EnableLogging { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
    }
}
