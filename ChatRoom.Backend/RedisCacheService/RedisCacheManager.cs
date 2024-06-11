using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace RedisCacheService
{
    public class RedisCacheManager(IDistributedCache cache) : IRedisCacheManager
    {
        private readonly IDistributedCache _cache = cache;
        public T GetCachedData<T>(string key)
        {
            var jsonData = _cache.GetString(key);
            if (jsonData == null)
                return default!;
            return JsonSerializer.Deserialize<T>(jsonData!)!;
        }
        public void SetCachedData<T>(string key, T data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration
            };

            var jsonData = JsonSerializer.Serialize(data);
            _cache.SetString(key, jsonData, options);
        }

        public bool RemoveData(string key)
        {
            _cache.Remove(key);
            var jsonData = _cache.GetString(key);
            if (jsonData == null)
                return true;
            return false;
        }

    }
}
