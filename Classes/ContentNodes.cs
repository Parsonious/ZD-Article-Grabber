using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

namespace ZD_Article_Grabber.Classes
{
    public class ContentNodes
    {
        public List<Node> Nodes { get; private set; }
        
        public ContentNodes(HtmlDocument doc, Dictionary<string, string> xpathDictionary, string baseUrl)
        {
            ArgumentNullException.ThrowIfNull(doc,nameof(doc));
                                  //get xpath search text from xpathDictionary
            Nodes = doc.DocumentNode.SelectNodes(string.Join("|", xpathDictionary.Keys))
                //get HtmlNode info from Node type
                ?.Select(htmlNode => new Node(htmlNode, baseUrl)).ToList()
                ?? new List<Node>();
        }

        //process and apply the local paths in parallel
        public async Task ProcessNodesAsync(IMemoryCache cache, IHttpContextAccessor accessor, IHttpClientFactory clientFactory)
        {
            var scheme = accessor.HttpContext.Request.Scheme;
            var host = accessor.HttpContext.Request.Host.Value;

            var tasks = Nodes.Select(async node =>
            {
                try
                {
                    string localPath = await ProcessFileAsync(cache, scheme, host, clientFactory, node);
                    node.SetLocalPath(localPath);
                }
                catch ( Exception ex )
                {
                    string defaultPath = $"{scheme}://{host}/a/c/{node.Type}/default";
                    node.SetLocalPath(defaultPath);
                }
            });

            await Task.WhenAll(tasks);
        }
        private async Task<string> ProcessFileAsync(IMemoryCache cache, string scheme, string host, IHttpClientFactory clientFactory, Node node)
        {
            var client = clientFactory.CreateClient();

            var fileType = node.Type;
            var fileUrl = node.FileUrl;
            var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

            string cacheKey = $"{fileType}_{fileName}";
            try
            {
                if ( !cache.TryGetValue(cacheKey, out byte[] fileContent) )
                {
                    var response = await client.GetAsync(fileUrl);
                    if ( response.IsSuccessStatusCode )
                    {
                        fileContent = await response.Content.ReadAsByteArrayAsync();
                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));
                        cache.Set(cacheKey, fileContent, cacheEntryOptions);
                    }
                    else
                    {
                        fileContent = await GetDefaultResourceAsync(cache, fileType);
                        cache.Set(cacheKey, fileContent);
                    }
                }
                return $"{scheme}://{host}/a/c/{fileType}/{Uri.EscapeDataString(fileName)}";
            }
            catch ( Exception ex )
            {
                // Use default resource
                string defaultFileName = GetDefaultFileName(fileType);
                string defaultCacheKey = $"{fileType}_{defaultFileName}";

                if ( !cache.TryGetValue(defaultCacheKey, out byte[] defaultFileContent) )
                {
                    defaultFileContent = await GetDefaultResourceAsync(cache, fileType);
                    cache.Set(defaultCacheKey, defaultFileContent);
                }

                return $"{scheme}://{host}/a/c/{fileType}/{Uri.EscapeDataString(defaultFileName)}";

            }
        }
        private async Task<byte[]> GetDefaultResourceAsync(IMemoryCache cache, string fileType)
        {
            string defaultCacheKey = $"{fileType}_default";

            if ( cache.TryGetValue(defaultCacheKey, out byte[] defaultContent) )
            {
                return defaultContent;
            }

            string defaultFileName = GetDefaultFileName(fileType);

            string defaultFilePath = Path.Combine(AppContext.BaseDirectory, "Defaults", defaultFileName);

            if ( !File.Exists(defaultFilePath) )
            {
                throw new FileNotFoundException($"Default resource not found: {defaultFilePath}");
            }

            defaultContent = await File.ReadAllBytesAsync(defaultFilePath);

            // Cache the default content
            cache.Set(defaultCacheKey, defaultContent, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            });

            return defaultContent;
        }
        private static string GetDefaultFileName(string fileType)
        {
            return fileType switch
            {
                "css" => "default.css",
                "js" => "default.js",
                "img" => "default.png",
                _ => throw new InvalidOperationException($"Unsupported File Type: {fileType}")
            };
        }
    }
}
