using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

namespace ZD_Article_Grabber.Classes
{
    public class ContentNodes
    {
        public List<Node> Nodes { get; private set; }
        
        public ContentNodes(HtmlDocument doc, Dictionary<string, string> xpathDictionary)
        {
            ArgumentNullException.ThrowIfNull(doc,nameof(doc));
                                  //get xpath search text from xpathDictionary
            Nodes = doc.DocumentNode.SelectNodes(string.Join("|", xpathDictionary.Keys))
                //get HtmlNode info from Node type
                ?.Select(htmlNode => new Node(htmlNode)).ToList()
                ?? new List<Node>();
        }

        //process and apply the local paths
        public async Task ProcessNodesAsync(HttpClient client, IMemoryCache cache, IHttpContextAccessor accessor)
        {
            foreach ( var node in Nodes )
            {
                string localPath = await ProcessFileAsync(client, cache, accessor, node.FileUrl, node.Type);
                node.SetLocalPath(localPath);
            }
        }

        private async Task<string> ProcessFileAsync(HttpClient client, IMemoryCache cache, IHttpContextAccessor accessor, string fileUrl, string fileType)
        {

            var scheme = accessor.HttpContext.Request.Scheme;
            var host = accessor.HttpContext.Request.Host;
            var baseUrl = $"{scheme}://{host}";

            var resolvedUrl = new Uri(new Uri(baseUrl), fileUrl).ToString();
            var fileName = Path.GetFileName(resolvedUrl);

            string cacheKey = $"{fileType}_{fileName}";

            if ( !cache.TryGetValue(cacheKey, out byte[] fileContent) )
            {
                var response = await client.GetAsync(resolvedUrl);
                if ( response.IsSuccessStatusCode )
                {
                    fileContent = await response.Content.ReadAsByteArrayAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));
                    cache.Set(cacheKey, fileContent, cacheEntryOptions);
                }
            }


            return $"{scheme}://{host}/a/c/{fileType}/{Uri.EscapeDataString(fileName)}";
        }
    }
}
