

namespace RedisCacheService
{
    public interface IRedisCacheManager
    {
        Task<T> GetCachedDataAsync<T>(string key);
        Task SetCachedDataAsync<T>(string key, T data, TimeSpan cacheDuration);
        Task SetCachedDataWithAbsoluteExpAsync<T>(string key, T data, TimeSpan cacheDuration);
        Task RemoveDataAsync(string key);
        

    }
}
