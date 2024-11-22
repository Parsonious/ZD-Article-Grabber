
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
            byte[] content = []; //initialize content to empty byte array for ResourceResult
            ResourceResult resource = new(id, content);
            Console.WriteLine($"Cache Stats: {_dependencies.Cache.GetCurrentStatistics()}");
            if ( _dependencies.Cache.TryGetValue(id.CacheKey, out byte[] fileContent) )
            {
                resource.Content = fileContent;
                resource.Url = id.ResourceUrl; //if this was already cached then the correct url was already set

                return resource;
            }
            if ( File.Exists(id.LocalUrl) )
            {
                resource.Content = await File.ReadAllBytesAsync(id.LocalUrl);
                resource.Url = id.LocalUrl; 
            }
            else // Fetch from remote URL
            {
                resource.Content = await FetchRemoteResourceAsync(id.RemoteUrl);
                resource.Url = id.RemoteUrl;
            }

            //Fallback to default resource if no content is found
            if ( resource.Content == null || resource.Content.Length == 0 )
            {
                resource.Content = await GetDefaultResourceAsync(id); //pass enitre ResourceID in order to set ResourceUrl to default file path
            }


            // Cache the content
            //var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));
            //_cache.Set( resource.Id.CacheKey, resource.Content, cacheEntryOptions);
            var cacheEntry = _dependencies.Cache.CreateEntry(id.CacheKey);
            cacheEntry.Value = resource.Content; // Set the value of the cache entry
            cacheEntry.SetSlidingExpiration(TimeSpan.FromMinutes(10));
            cacheEntry.Dispose(); // Ensure the entry is committed to the cache

            return resource;
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

            return []; // Return an empty byte array if fetching fails
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
