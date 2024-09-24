using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using ZD_Article_Grabber.Common;
using ZD_Article_Grabber.Classes;


namespace ZD_Article_Grabber
{
    public class Fetch(IMemoryCache cache, HttpClient client, IHttpContextAccessor accessor, String sourceUrl = "https://parsonious.github.io/How-To/pages/")
    {
        private readonly IMemoryCache _cache = cache;
        private readonly HttpClient _client = client;
        private readonly IHttpContextAccessor _contextAccessor = accessor;
        private readonly String _sourceUrl = sourceUrl;

        public async Task<string> FetchAndModifyHtmlAsync(string title)
        {
            //normalize the title
           string normalizedTitle = NormalizeTitle(title);
            string cacheKey = $"content_{normalizedTitle}";

            //check if the html is cached
            if ( !_cache.TryGetFromCache(cacheKey, out string htmlContent) )
            {
                //fetch the html if not cached
                string pageUrl = $"{_sourceUrl}{normalizedTitle}.html";

                htmlContent = await _client.GetStringAsync(pageUrl) ?? throw new Exception("Page Not Found");

                var content = new Content(_cache, _client, _contextAccessor, htmlContent, pageUrl);
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
            if(title.Contains(" "))
            return title.Replace(" ", "%20");
            else return title;
        }
        private async Task<string> GetContentFromUrlAsync(string url)
        {
            try
            {
                return await _client.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                return ex.Message; //placeholder for better error handling
            }
        }

        private async Task<string> ProcessHtmlAndAssetsAsync(string htmlContent, string baseUrl)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(htmlContent); //replace with HtmlDocument.Load for by-file loading instead

            HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//link[@rel='stylesheet'] | //script[@src]");

            if ( nodes == null )
            {
                return htmlDoc.DocumentNode.OuterHtml; //nothing to process
            }
            foreach ( var node in nodes )
            {
                string fileUrl = node.Name == "link"
                    ? node.GetAttributeValue("href", string.Empty)
                    : node.GetAttributeValue("src", string.Empty);

                string fileType = node.Name == "link" ? "css" : "js";

                if ( !string.IsNullOrEmpty(fileUrl) )
                {
                    //Fetch and cache the CSS/JS file
                    var cachedFilePath = await CacheCssOrJsFileAsync(fileUrl, fileType, baseUrl);

                    //update the node with the new cached file path
                    if ( fileType == "css" )
                        node.SetAttributeValue("href", cachedFilePath);
                    else if ( fileType == "js" )
                        node.SetAttributeValue("src", cachedFilePath);
                }
            }
            return htmlDoc.DocumentNode.OuterHtml;
        }
        private async Task<string> CacheCssOrJsFileAsync(string fileUrl, string fileType, string baseUrl)
        {
            //get protocol and host domain
            var protocol = _contextAccessor.HttpContext.Request.Scheme;
            var host = _contextAccessor.HttpContext.Request.Host.Value;

            //Resolve full URL
            string resolvedUrl = new Uri(new Uri(baseUrl), fileUrl).ToString();
            string fileName = Path.GetFileName(resolvedUrl);
            string cacheKey =$"{fileType}_{fileName}";

            // Check if CSS/JS content is already cached
            if( !_cache.TryGetValue(cacheKey,out string fileContent))
            {
                fileContent = await GetContentFromUrlAsync(resolvedUrl);
                if (fileContent == null)
                {
                    return fileUrl; // Fallback to original URL if fetch fails
                }

                // Cache the file content
                _cache.Set(cacheKey, fileContent, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
            }

            // serve cached file from Memory via a controller endopoint
            return $"{protocol}://{host}/a/c/{fileType}/{Uri.EscapeDataString(fileName)}";
        }

        
    }

}
