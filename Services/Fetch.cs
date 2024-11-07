using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using ZD_Article_Grabber.Common;
using System.IO;
using Microsoft.Extensions.Options;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Types;
using System.Text;
using ZD_Article_Grabber.Interfaces;


namespace ZD_Article_Grabber.Services
{
    public class Fetch(IMemoryCache cache,
        IHttpClientFactory clientFactory,
        IHttpContextAccessor accessor,
       IConfigOptions defaultPaths,
       IResourceFetcher resourceFetcher)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly IHttpClientFactory _clientFactory = clientFactory;
        private readonly IHttpContextAccessor _contextAccessor = accessor;
        private readonly IConfigOptions _defaultPaths = defaultPaths;
        private readonly IResourceFetcher _resourceFetcher = resourceFetcher;

        public async Task<string> FetchHtmlAsync(string title)
        {
            Console.WriteLine(_defaultPaths.Paths.UrlPath + "\n" + _defaultPaths.Paths.HtmlFilesPath + "\n" + _defaultPaths.Paths.ResourceFilesPath);
            //Set up necessary variables
            string normalizedTitle = PathHelper.NormalizeTitle(title),
                   cacheKey = CacheHelper.GenerateCacheKey("html", normalizedTitle),//at this stage this should be fetching the html file as the main file ergo hardcoded is k
                   localFilePath = Path.Combine(_defaultPaths.Paths.HtmlFilesPath,title),
                   pageUrl = $"{_defaultPaths.Paths.UrlPath}{PathHelper.GetUrlTitle(title)}.html";

            if ( _cache.TryGetFromCache(cacheKey, out string htmlContent) )
            {
                return htmlContent;
            }

            // Use ResourceFetcher to get the HTML content
            byte[] contentBytes = await resourceFetcher.FetchResourceAsync("html", $"{normalizedTitle}.html", pageUrl);

            if ( contentBytes == null )
            {
                throw new FileNotFoundException($"HTML content for '{title}' could not be found locally or remotely.");
            }

            htmlContent = Encoding.UTF8.GetString(contentBytes);

            var content = new Content(_cache, _clientFactory, _contextAccessor, htmlContent, pageUrl, _defaultPaths, _resourceFetcher);
            await content.ProcessFilesAsync();

            htmlContent = content.HtmlDoc.DocumentNode.OuterHtml;

            _cache.SetCache(cacheKey, htmlContent, TimeSpan.FromMinutes(10));

            return htmlContent;
        }
    }

}
