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

        public async Task SetCachedDataAsync<T>(string key, T data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = cacheDuration
            };

            var jsonData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(key, jsonData, options);
        }
        public async Task SetCachedDataWithAbsoluteExpAsync<T>(string key, T data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(cacheDuration)
            };

            var jsonData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        public async Task RemoveDataAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

    }
}
