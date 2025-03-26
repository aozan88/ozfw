namespace KonFwCaching
{
    public interface IKonCache
    {
        Task<T> GetAsync<T>(string key);
        Task<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task ClearAllAsync();
    }
}
