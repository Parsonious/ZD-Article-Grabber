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
                //Update the cache Expiration for the page to be in cache for 10 additional minutes. 
                //This approach can be dangerous if the page is accessed or updated frequently but works for now.
                //Will need to create a new Issue for this to improve on Cache Expiration and prevent stale data.
                TimeSpan remainingTime = cachedPage.Expiration - System.DateTimeOffset.Now;
                if ( remainingTime < TimeSpan.FromMinutes(10))
                {
                    cachedPage.Expiration = System.DateTimeOffset.Now.AddMinutes(10);

                    _dependencies.Cache.Set(iD.CacheKey, cachedPage, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = cachedPage.Expiration
                    });
                }
                return cachedPage.Page;
            }

            // Use ResourceFetcher to get the HTML content
            ResourceResult rawResource = await _resourceFetcher.FetchResourceAsync(iD) ?? throw new InvalidOperationException("Content Could Not Be Fetched");
            
            //Since we are returning the HTML as a string, convert byte[] to string
            string stringHtml = System.Text.Encoding.UTF8.GetString(rawResource.Content);
            iD.ResourceUrl = rawResource.Url;

            //Build Page
            Page page = new(iD, stringHtml, _dependencies, _nodeBuilder);
            
            //Build nodes of page
            await page.ProcessFilesAsync();

            //Cache the processed page
            _dependencies.Cache.Set(page.Id.CacheKey, page.Html); //make sure to set cache with updated HTML from Page, not raw from ResourceFetcher
            return page;
        }

    }
}