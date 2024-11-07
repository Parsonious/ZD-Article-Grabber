using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Services
{
    public class ResourceFetcher : IResourceFetcher
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfigOptions _configOptions;

        public ResourceFetcher(IMemoryCache cache, IHttpClientFactory clientFactory, IConfigOptions configOptions)
        {
            _cache = cache;
            _clientFactory = clientFactory;
            _configOptions = configOptions;
        }

        public async Task<byte[]> FetchResourceAsync(string fileType, string fileName, string remoteUrl)
        {
            string cacheKey = CacheHelper.GenerateCacheKey(fileType, fileName);

            if ( _cache.TryGetValue(cacheKey, out byte[] fileContent) )
            {
                return fileContent;
            }

            // Try fetching from local path
            string localFilePath = Path.Combine(_configOptions.Paths.ResourceFilesPath, fileType, fileName);

            if ( File.Exists(localFilePath) )
            {
                fileContent = await File.ReadAllBytesAsync(localFilePath);
            }
            else
            {
                // Fetch from remote URL
                fileContent = await FetchRemoteResourceAsync(remoteUrl);
            }

            if ( fileContent == null )
            {
                // Fetch default resource
                fileContent = await GetDefaultResourceAsync(fileType);
            }

            // Cache the content
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));
            _cache.Set(cacheKey, fileContent, cacheEntryOptions);

            return fileContent;
        }

        private async Task<byte[]> FetchRemoteResourceAsync(string url)
        {
            try
            {
                var client = _clientFactory.CreateClient();
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

            return null;
        }

        private async Task<byte[]> GetDefaultResourceAsync(string fileType)
        {
            string defaultFileName = GetDefaultFileName(fileType);
            string defaultFilePath = Path.Combine(AppContext.BaseDirectory, "Defaults", defaultFileName);

            if ( !File.Exists(defaultFilePath) )
            {
                throw new FileNotFoundException($"Default resource not found: {defaultFilePath}");
            }

            return await File.ReadAllBytesAsync(defaultFilePath);
        }

        private string GetDefaultFileName(string fileType)
        {
            if ( _configOptions.Files.DefaultFiles.TryGetValue(fileType, out var defaultFileName) )
            {
                return defaultFileName;
            }

            throw new InvalidOperationException($"Unsupported File Type: {fileType}");
        }
    }

}
