using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Helpers;

namespace ZD_Article_Grabber.Controllers
{
    [Route("a/c/{fileType}/{fileName}")]
    [ApiController]
    public class CacheController(IMemoryCache _cache) : ControllerBase
    {
        private readonly IMemoryCache _cache = _cache;

        [HttpGet]
        public IActionResult GetCachedFile(string fileType, string fileName)
        {
            string cacheKey = CacheHelper.GenerateCacheKey(fileType,fileName);

            if ( _cache.TryGetValue(cacheKey, out byte[] fileContent) )
            {
                string contentType = GetContentType(fileName);
                return File(fileContent, contentType);
            }

            return NotFound("File not found in cache");
        }
    
        private static string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if ( !provider.TryGetContentType(fileName, out string contentType) )
            {
                // Manually handle known types
                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                contentType = GetTypeByExtension(extension);
            }
            return contentType;
        }
        private static string GetTypeByExtension(string extension)
        {
            return extension switch
            {
                ".sql" => "application/sql",
                ".psq" => "application/x-powershell",
                _ => "application/octet-stream",
            };
        }
    }
}
