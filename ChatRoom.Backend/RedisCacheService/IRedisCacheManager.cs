

namespace RedisCacheService
{
    public interface IRedisCacheManager
    {
        T GetCachedData<T>(string key);
        void SetCachedData<T>(string key, T data, TimeSpan cacheDuration);
        bool RemoveData(string key);
    }
}
