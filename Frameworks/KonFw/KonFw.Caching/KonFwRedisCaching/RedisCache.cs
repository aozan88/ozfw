using KonFwCaching;

namespace KonFwRedisCaching
{
    public class RedisCache : IKonCache
    {
        private readonly RedisCacheConfiguration _config;

        public RedisCache(RedisCacheConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (!_config.IsEnabled) return default;
            // Real implementation would use StackExchange.Redis
            Console.WriteLine($"[Redis GET] {_config.ConnectionString} - Key: {key}");
            return default;
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (!_config.IsEnabled) return await factory();

            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null) return cachedValue;

            var newValue = await factory();

            if (newValue != null)
            {
                await SetAsync(key, newValue, expiration);
            }

            return newValue;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (!_config.IsEnabled) return;
            var exp = expiration ?? _config.DefaultExpiration;
            Console.WriteLine($"[Redis SET] {_config.ConnectionString} - Key: {key} - Expiration: {exp}");
        }

        public async Task RemoveAsync(string key)
        {
            if (!_config.IsEnabled) return;
            Console.WriteLine($"[Redis REMOVE] {_config.ConnectionString} - Key: {key}");
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (!_config.IsEnabled) return false;
            Console.WriteLine($"[Redis EXISTS] {_config.ConnectionString} - Key: {key}");
            return false;
        }
        public async Task RemoveByPatternAsync(string pattern)
        {
            if (!_config.IsEnabled) return;

            Console.WriteLine($"[Redis REMOVE PATTERN] {_config.ConnectionString} - Pattern: {pattern}");
        }

        public async Task ClearAllAsync()
        {
            if (!_config.IsEnabled) return;

            Console.WriteLine($"[Redis CLEAR ALL] {_config.ConnectionString}");
        }
    }
}
