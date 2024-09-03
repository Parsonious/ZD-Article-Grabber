using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace ZD_Article_Grabber
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public ContentController(IMemoryCache cache) 
        {
            _cache = cache; 
        }

        [HttpGet("{title}")]

        public IActionResult GetContent(string title)
        {
            // Normalize title to match your file naming convention
            string normalizedTitle = title.Replace(" ", "%20");
            string cacheKey = $"content_{normalizedTitle}";


            if(!_cache.TryGetValue(cacheKey, out string htmlContent))
            {
                // Cache not found => Fetch from external source
                string url = $"https://parsonious.github.io/CIQ-How-To/pages/{normalizedTitle}.html";
                using(var client = new HttpClient() )
                {
                    // Determine result from client
                    var response = client.GetAsync(url).Result;
                    if(response.IsSuccessStatusCode)
                    {
                        htmlContent = response.Content.ReadAsStringAsync().Result;

                        // Set cache options
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(10)); //adjust for cache duration
                        
                        // Save data in cache
                        _cache.Set(cacheKey, htmlContent, cacheEntryOptions);
                    }
                    else
                    {
                        return NotFound("Page not found");
                    }
                }
            }
            return Content(htmlContent, "text/html");
           
        }
    }

}
