

namespace RedisCacheService
{
    public interface IRedisCacheManager
    {
        Task<T> GetCachedDataAsync<T>(string key);
        void SetCachedData<T>(string key, T data, TimeSpan cacheDuration);
        void SetCachedDataWithAbsoluteExp<T>(string key, T data, TimeSpan cacheDuration);
        Task RemoveDataAsync(string key);


    }
}
