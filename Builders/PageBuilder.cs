using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Resources;
using ZD_Article_Grabber.Resources.Pages;
using ZD_Article_Grabber.Resources.Cache;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Builders
{
    public class PageBuilder(Dependencies dependencies, IResourceFetcher resourceFetcher, INodeBuilder nodeBuilder) : IPageBuilder
    {
        readonly Dependencies _dependencies = dependencies;
        readonly INodeBuilder _nodeBuilder = nodeBuilder;
        readonly IResourceFetcher _resourceFetcher = resourceFetcher;

        public async Task<Page> BuildPageAsync(string title)
        {
            // Set up necessary variables
            PageID iD = new(title, _dependencies.Settings, _dependencies.PathHelper);

            // Check if the page is already in the cache
            if ( _dependencies.Cache.TryGetValue(iD.CacheKey, out CachedPage? cachedPage) && cachedPage is not null )
            {
                TimeSpan remainingTime = cachedPage.Expiration - DateTimeOffset.Now;
                if ( remainingTime < TimeSpan.FromMinutes(10))
                {
                    cachedPage.Expiration = DateTimeOffset.Now.AddMinutes(10);

                    _dependencies.Cache.Set(iD.CacheKey, cachedPage, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = cachedPage.Expiration,
                        Size = 2048
                    });
                }
                return cachedPage.Page;
            }

            // Use ResourceFetcher to get the HTML content
            ResourceResult rawResource = await _resourceFetcher.FetchResourceAsync(iD) ?? throw new InvalidOperationException("Content Could Not Be Fetched");
            
            /// TODO: This took 548ms to process NLA Whitelisting...this needs to be faster.
            //Since we are returning the HTML as a string, convert byte[] to string 
            string stringHtml = System.Text.Encoding.UTF8.GetString(rawResource.Content);
            iD.ResourceUrl = rawResource.Url;

            //Build Page
            Page page = new(iD, stringHtml, _dependencies, _nodeBuilder);
            /// TODO: This took 646ms to process NLA Whitelisting...this needs to be faster.
            //Build nodes of page
            await page.ProcessFilesAsync();

            CachedPage cache = new (page, DateTimeOffset.Now.AddMinutes(10));
            //Cache the processed page
            _dependencies.Cache.Set(page.Id.CacheKey, cache, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Size = page.Html.Length * 2
            }); //make sure to set cache with updated HTML from Page, not raw from ResourceFetcher
            return page;
        }

    }
}