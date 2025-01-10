
using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Resources;
using ZD_Article_Grabber.Types;


namespace ZD_Article_Grabber.Services
{
    public class ResourceFetcher(Dependencies dependencies) : IResourceFetcher
    {
        private readonly Dependencies _dependencies = dependencies;
        public async Task<ResourceResult> FetchResourceAsync(ResourceID id)
        {
            byte[] content; //initialize content to empty byte array for ResourceResult
            string url;
            if ( _dependencies.Cache.TryGetValue(id.CacheKey, out ResourceResult? cachedResource) && cachedResource is not null)
            {
                return cachedResource;
            }
            if ( File.Exists(id.LocalUrl) )
            {
                content = await File.ReadAllBytesAsync(id.LocalUrl);
                url = id.LocalUrl; 
            }
            else // Fetch from remote URL
            {
                content = await FetchRemoteResourceAsync(id.FallbackRemoteUrl);
                url = id.FallbackRemoteUrl;
            }

            //Fallback
            if ( content.Length == 0 )
            {
                content = await GetDefaultResourceAsync(id); 
                url = id.ResourceUrl; //This is set in GetDefaultResourceAsync
            }

            ResourceResult resourceResult = new(id, content)
            {
                Url = url
            };
            // Cache the content
            _dependencies.Cache.Set(id.CacheKey, resourceResult, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });

            return resourceResult;
        }
        private async Task<byte[]> FetchRemoteResourceAsync(string url)
        {
            try
            {
                var client = _dependencies.ClientFactory.CreateClient();
                var response = await client.GetAsync(url);

                if ( response.IsSuccessStatusCode )
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch ( HttpRequestException ex )
            {
                Console.WriteLine($"Failed to fetch remote resource '{url}': {ex.Message}");
            }

            return Array.Empty<byte>(); // Leave as the fully declared call to return a singleton empty array instead of making a new one each time.
        }

        private async Task<byte[]> GetDefaultResourceAsync(ResourceID iD)
        {
            string defaultFileName = GetDefaultFileName(iD.Type);
            string defaultFilePath = Path.Combine(_dependencies.Settings.Paths.ResourceFilesPath, iD.Type.ToString().ToLower(), defaultFileName);

            if ( !File.Exists(defaultFilePath) )
            {
                throw new FileNotFoundException($"Default resource not found: {defaultFilePath}");
            }

            iD.ResourceUrl = defaultFilePath;
            return await File.ReadAllBytesAsync(defaultFilePath);
        }

        private string GetDefaultFileName(ResourceType type)
        {
            if ( _dependencies.Settings.Files.DefaultFiles.TryGetValue(type.ToString().ToLower(), out var defaultFileName) )
            {
                return defaultFileName;
            }

            throw new InvalidOperationException($"Unsupported File Type: {type}");
        }
    }

}
