using Microsoft.Extensions.Caching.Memory;

namespace ZD_Article_Grabber.Common
{
    public static class CacheExtensions
    {
        public static bool TryGetFromCache<T>(this IMemoryCache cache, string cacheKey, out T value)
        {
            if ( cache.TryGetValue(cacheKey, out object? cachedValue) )
            {
                value = (T) cachedValue;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public static void SetCache<T>(this IMemoryCache cache, string cacheKey, T value, TimeSpan expiration)
        {
            cache.Set(cacheKey, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = expiration
            });
        }
    }
}
