using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ZD_Article_Grabber
{
    public class Fetch(IMemoryCache cache, HttpClient client)
    {
        private readonly IMemoryCache _cache = cache;
        private readonly HttpClient _client = client;


        public async Task<string> FetchAndModifyHtmlAsync(string title)
        {
            //normalize the title
            string normalizedTitle = title.Replace(" ", "%20");
            string cacheKey = $"content_{normalizedTitle}";

            //check if the html is cached
            if ( !_cache.TryGetValue(cacheKey, out string htmlContent) )
            {
                //fetch the html if not cached
                string url = $"https://parsonious.github.io/How-To/pages/{normalizedTitle}.html";
                htmlContent = await GetContentFromUrlAsync(url);

                if ( htmlContent == null )
                {
                    return "Page Not Found";
                }

                //modify the html to replace references to CSS/JS with cached versions
                htmlContent = await ProcessHtmlAndAssetsAsync(htmlContent, url);

                // Cache the modified HTML for 10 mins
                _cache.Set(cacheKey, htmlContent, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
            }

            return htmlContent;
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
            //Resolve full URL
            string resolvedUrl = new Uri(new Uri(baseUrl), fileUrl).ToString();
            string cacheKey =$"{fileType}_{resolvedUrl}";

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
            return $"/a/c/{fileType}/{Uri.EscapeDataString(Path.GetFileName(resolvedUrl))}";
        }

        
    }

}
