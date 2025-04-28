
using System.Buffers;
using System.IO.Pipelines;
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
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Size = (content.Length * 2)
            });

            return resourceResult;
        }
        private async Task<byte[]> FetchRemoteResourceAsync(string url)
        {
            try
            {
                using var client = _dependencies.ClientFactory.CreateClient();
                using var response = await client.GetAsync(url);

                if ( !response.IsSuccessStatusCode )
                {
                    return Array.Empty<byte>(); // Leave as the fully declared call to return a singleton empty array instead of making a new one each time.
                }

                int length = (int) response.Content.Headers.ContentLength.GetValueOrDefault();

                //use ArrayPool for small files
                if ( length < 85_000 ) //85kb
                {
                    return await FetchWithArrayPool(response, length);
                }

                //use Pipe for large files
                return await FetchWithPipelines(response);
            }
            catch ( HttpRequestException ex )
            {
                Console.WriteLine($"Failed to fetch remote resource '{url}': {ex.Message}");
                return Array.Empty<byte>();
            }
        }
        private static async Task<byte[]> FetchWithPipelines(HttpResponseMessage response)
        { 
            Pipe pipe = new();
            await using var stream = await response.Content.ReadAsStreamAsync();

            using MemoryStream memStream = new();

            async Task FillPipeAsync()
            {
                try
                {
                    while ( true )
                    {
                        Memory<byte> memory = pipe.Writer.GetMemory(4096);
                        int bytesRead = await stream.ReadAsync(memory);
                        if ( bytesRead == 0 )
                        {
                            break;
                        }

                        pipe.Writer.Advance(bytesRead);
                        FlushResult result = await pipe.Writer.FlushAsync();

                        if ( result.IsCompleted )
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    await pipe.Writer.CompleteAsync();
                }
            }

            async Task ReadPipeAsync()
            {
                try
                {
                    while ( true )
                    {
                        ReadResult result = await pipe.Reader.ReadAsync();
                        ReadOnlySequence<byte> buffer = result.Buffer;

                        foreach ( ReadOnlyMemory<byte> segment in buffer )
                        {
                            await memStream.WriteAsync(segment);
                        }

                        pipe.Reader.AdvanceTo(buffer.End);

                        if ( result.IsCompleted )
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    await pipe.Reader.CompleteAsync();
                }
            }

            //run both concurrently
            await Task.WhenAll(FillPipeAsync(), ReadPipeAsync());
            return memStream.ToArray();
        }
        private static async Task<byte[]> FetchWithArrayPool(HttpResponseMessage response, int length)
        { 
            //Use ArrayPool for large buffers
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, length));
                byte[] result = new byte[bytesRead];

                Buffer.BlockCopy(buffer, 0, result, 0, bytesRead);
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private async Task<byte[]> GetDefaultResourceAsync(ResourceID iD)
        {
            string defaultFileName = GetDefaultFileName(iD.Type);
            string defaultFilePath = Path.Combine(_dependencies.Settings.Paths.ResourceFilesPath, iD.Type.ToString().ToLower(), defaultFileName);

            if ( !File.Exists(defaultFilePath) )
            {
                throw new FileNotFoundException($"Default resource not found: {defaultFilePath}");
            }

            // Update the ResourceUrl property directly
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
