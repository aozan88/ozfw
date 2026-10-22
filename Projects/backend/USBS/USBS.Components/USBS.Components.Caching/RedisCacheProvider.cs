using OzFwCaching;
using OzFwRedisCaching;
namespace USBS.Components.Caching
{

    public class RedisCacheProvider : RedisCache
    {
        private readonly IKonCache _cache;
        private readonly RedisCacheConfiguration _config;
        public RedisCacheProvider(RedisCacheConfiguration config) : base(config)
        {
            _config = config;
        }

        public IKonCache GetCache() => _cache;
    }
}
