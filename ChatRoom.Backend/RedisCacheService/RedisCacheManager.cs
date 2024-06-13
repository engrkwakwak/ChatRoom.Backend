using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RedisCacheService
{
    public class RedisCacheManager(IDistributedCache cache) : IRedisCacheManager
    {
        private readonly IDistributedCache _cache = cache;
        public async Task<T> GetCachedDataAsync<T>(string key)
        {
            var jsonData = await _cache.GetStringAsync(key);
            if (jsonData == null)
                return default!;
            return JsonSerializer.Deserialize<T>(jsonData!)!;
        }

        public void SetCachedData<T>(string key, T data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = cacheDuration
            };

            var jsonData = JsonSerializer.Serialize(data);
            _cache.SetString(key, jsonData, options);
        }

        public async Task RemoveDataAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

    }
}
