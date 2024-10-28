using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using ZD_Article_Grabber.Common;
using ZD_Article_Grabber.Classes;
using System.IO;
using Microsoft.Extensions.Options;


namespace ZD_Article_Grabber
{
    public class Fetch(IMemoryCache cache, 
        IHttpClientFactory clientFactory, 
        IHttpContextAccessor accessor,
        IOptions<AppSetPaths> defaultPaths)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly IHttpClientFactory _clientFactory = clientFactory;
        private readonly IHttpContextAccessor _contextAccessor = accessor;
        private readonly AppSetPaths _defaultPaths = defaultPaths.Value;

        public async Task<string> FetchHtmlAsync(string title)
        {
            //normalize the title
           string normalizedTitle = NormalizeTitle(title),
                  cacheKey = $"content_{normalizedTitle}",
                  localFilePath = Path.Combine(_defaultPaths.HtmlFilesPath, title),
                  pageUrl = $"{_defaultPaths.UrlPath}{normalizedTitle}.html";

            //check if the html is cached
            if ( !_cache.TryGetFromCache(cacheKey, out string htmlContent) )
            {     

                if ( File.Exists(localFilePath) ) //if local file is found
                {
                    //read content from local file
                    htmlContent = await File.ReadAllTextAsync(localFilePath);
                }
                else //else check for file at _sourceUrl
                {
                    var client = _clientFactory.CreateClient();
                    //fetch the html if not cached


                    htmlContent = await client.GetStringAsync(pageUrl) ?? throw new Exception("Page Not Found");

                }

                var content = new Content(_cache,_clientFactory, _contextAccessor, htmlContent, pageUrl);
                await content.ProcessFilesAsync();

                //update htmlContent variable with processed html
                htmlContent = content.HtmlDoc.DocumentNode.OuterHtml;

                // Cache the modified HTML for 10 mins
                _cache.SetCache(cacheKey, htmlContent, TimeSpan.FromMinutes(10));
            }

            return htmlContent;
        }
        private static string NormalizeTitle(string title)
        {
            if(title.Contains(' '))
            return title.Replace(" ", "%20");
            else return title;
        }
    }

}
