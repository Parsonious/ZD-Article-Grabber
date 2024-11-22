
using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Interfaces;
using System;

namespace ZD_Article_Grabber.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public bool TryGet<TItem>(object key, out TItem value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public void Set<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        {
            _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
        }
    }
}