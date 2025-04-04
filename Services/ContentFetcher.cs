using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Resources.Pages;

namespace ZD_Article_Grabber.Services
{
    public class ContentFetcher(IMemoryCache cache, IPageBuilder pageBuilder) : IContentFetcher
    {
        private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromMinutes(10);

        private readonly IMemoryCache _cache = cache;
        private readonly IPageBuilder _pageBuilder = pageBuilder;

        public async Task<string> FetchHtmlAsync(string title)
        {
            Page page = await _pageBuilder.BuildPageAsync(title);

            //set up cache options
            var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(DefaultCacheTime)
            .SetPriority(CacheItemPriority.Normal);

            // Cache the page
            _cache.Set(page.Id.CacheKey, page.Html, cacheOptions);

            //return the html of the page
            return page.Html;
        }
    }
}