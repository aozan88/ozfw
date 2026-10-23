using OzFwRedisCaching;

namespace USBS.Components.Caching
{
    /// <summary>Uygulamaya özgü Redis önbellek; framework'teki RedisCache'i genişletme noktası.</summary>
    public class RedisCacheProvider : RedisCache
    {
        public RedisCacheProvider(RedisCacheConfiguration config) : base(config)
        {
        }
    }
}
