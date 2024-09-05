using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            string cacheKey = $"{fileType}_{Uri.UnescapeDataString(fileName)}";

            if ( _cache.TryGetValue(cacheKey, out string fileContent) )
            {
                string contentType = fileType == "css" ? "text/css" : "application/javascript";
                return Content(fileContent, contentType);
            }
            return NotFound("File not found in cache");
        }
    }
}
