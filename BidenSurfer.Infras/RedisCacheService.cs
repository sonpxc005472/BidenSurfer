using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BidenSurfer.Infras
{
    public interface IRedisCacheService
    {
        T? GetCachedData<T>(string key);
        void SetCachedData<T>(string key, T data, TimeSpan cacheDuration);
        void RemoveCachedData(string key);
    }
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public T? GetCachedData<T>(string key)
        {
            var jsonData = _cache.GetString(key);
            if (jsonData == null)
                return default;
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public void RemoveCachedData(string key)
        {
            _cache.Remove(key);
        }

        public void SetCachedData<T>(string key, T data, TimeSpan cacheDuration)
        {
            var cachedData = GetCachedData<T>(key);
            if (cachedData != null)
            {
                _cache?.Remove(key);
            }
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration
            };
            var jsonData = JsonSerializer.Serialize(data);
            _cache.SetString(key, jsonData, options);
        }
    }
}
