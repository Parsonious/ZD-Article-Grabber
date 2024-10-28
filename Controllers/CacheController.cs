using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;

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
            string cacheKey = $"{fileType}_{fileName}";

            if ( _cache.TryGetValue(cacheKey, out byte[] fileContent) )
            {
                string contentType = GetContentType(fileName);
                return File(fileContent, contentType);
            }

            return NotFound("File not found in cache");
        }
    
        private string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if ( !provider.TryGetContentType(fileName, out string contentType) )
            {
                // Manually handle known types
                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                switch ( extension )
                {
                    case ".sql":
                        contentType = "application/sql";
                        break;
                    case ".ps1":
                        contentType = "application/x-powershell";
                        break;
                    default:
                        contentType = "application/octet-stream"; // default content type
                        break;
                }
            }
            return contentType;
        }
    }
}
