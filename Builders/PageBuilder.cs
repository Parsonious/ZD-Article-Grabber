using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Resources;
using ZD_Article_Grabber.Resources.Pages;
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

            // Use ResourceFetcher to get the HTML content
            ResourceResult rawResource = await _resourceFetcher.FetchResourceAsync(iD) ?? throw new InvalidOperationException("Content Could Not Be Fetched");
            
            //Since we are returning the HTML as a string, convert byte[] to string
            string stringHtml = System.Text.Encoding.UTF8.GetString(rawResource.Content);
            iD.ResourceUrl = rawResource.Url;

            //build Page
            Page page = new(iD, stringHtml, _dependencies, _nodeBuilder);
            //build nodes of page
            await page.ProcessFilesAsync();

            _dependencies.Cache.Set(page.Id.CacheKey, page.Html); //make sure to set cache with updated HTML from Page, not raw from ResourceFetcher
            return page;
        }

    }
}