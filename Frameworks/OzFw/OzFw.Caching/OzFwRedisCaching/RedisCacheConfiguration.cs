using OzFwCaching;

namespace OzFwRedisCaching
{
    public class RedisCacheConfiguration : CacheConfiguration
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "USBS:";
        public int Database { get; set; } = 0;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectRetry { get; set; } = 3;
        public int ConnectTimeout { get; set; } = 5000;
        public bool AllowAdmin { get; set; } = false;
    }
}
