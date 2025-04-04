using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;
namespace ZD_Article_Grabber.Common
{
    public static class CacheExtensions
    {
        public static bool TryGetFromCache<T>(this IMemoryCache cache, string cacheKey, [NotNullWhen(true)] out T? value)
            where T : class
        {
            if ( cache.TryGetValue(cacheKey, out object? cached) && cached is T typedValue)
            {
                value = typedValue;
                return true;
            }
            else
            {
                value = default;
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
