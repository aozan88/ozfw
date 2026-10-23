using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace OzFwCaching
{
    /// <summary>
    /// Süreç içi IKonCache (Cache:Provider = memory). Dağıtık değildir; yerel geliştirme, testler ve
    /// tek örnekli servisler için. Desen/tümünü silme için anahtarlar ayrıca izlenir.
    /// </summary>
    public class MemoryKonCache : IKonCache, IDisposable
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly ConcurrentDictionary<string, byte> _keys = new();
        private readonly CacheConfiguration _config;

        public MemoryKonCache(CacheConfiguration? config = null)
        {
            _config = config ?? new CacheConfiguration();
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (_config.IsEnabled && _cache.TryGetValue(key, out var value) && value is T typed)
                return Task.FromResult(typed);
            return Task.FromResult(default(T)!);
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (!_config.IsEnabled)
                return await factory();
            if (_cache.TryGetValue(key, out var value) && value is T typed)
                return typed;

            var created = await factory();
            await SetAsync(key, created, expiration);
            return created;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (!_config.IsEnabled)
                return Task.CompletedTask;

            var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration ?? _config.DefaultExpiration };
            options.RegisterPostEvictionCallback((k, _, _, _) => _keys.TryRemove((string)k, out _));
            _cache.Set(key, value, options);
            _keys[key] = 0;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key) => Task.FromResult(_cache.TryGetValue(key, out _));

        /// <summary>Redis KEYS deseni gibi: '*' herhangi bir dizi, '?' tek karakter.</summary>
        public Task RemoveByPatternAsync(string pattern)
        {
            var regex = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$");
            foreach (var key in _keys.Keys.Where(k => regex.IsMatch(k)))
                RemoveAsync(key);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync()
        {
            foreach (var key in _keys.Keys)
                RemoveAsync(key);
            return Task.CompletedTask;
        }

        public void Dispose() => _cache.Dispose();
    }
}
