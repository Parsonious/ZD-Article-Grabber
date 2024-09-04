using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace ZD_Article_Grabber
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly Fetch _fetchService; 

        public ContentController(IMemoryCache cache, Fetch fetchService)
        {
            _cache = cache;
            _fetchService = fetchService;
        }

        [HttpGet("{title}")]
        public async Task<IActionResult> GetContent(string title)
        {
            // Normalize title to match your file naming convention
            string normalizedTitle = title.Replace(" ", "%20");
            string cacheKey = $"content_{normalizedTitle}";

            if ( !_cache.TryGetValue(cacheKey, out string htmlContent) )
            {
                // Cache not found => Fetch from external source
                string url = $"https://parsonious.github.io/CIQ-How-To/pages/{normalizedTitle}.html";

                // Call FetchAndModifyHtmlAsync to fetch and modify the content
                htmlContent = await _fetchService.FetchAndModifyHtmlAsync(url);

                if ( !string.IsNullOrEmpty(htmlContent) )
                {
                    // Set cache options
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10)); //adjust for cache duration

                    // Save modified HTML in cache
                    _cache.Set(cacheKey, htmlContent, cacheEntryOptions);
                }
                else
                {
                    return NotFound("Page not found");
                }
            }

            // Return the modified HTML content with references to the locally cached CSS/JS
            return Content(htmlContent, "text/html");
        }
    }
}
