using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;



namespace ZD_Article_Grabber
{
    [Route("a/p")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public ContentController(IMemoryCache cache) 
        {
            _cache = cache; 
        }

        [HttpGet("{title}")]

        public async Task<IActionResult> GetContent(string title)
        {
            // Normalize title to match your file naming convention
            string normalizedTitle = title.Replace(" ", "%20");
            string cacheKey = $"content_{normalizedTitle}";


            if(!_cache.TryGetValue(cacheKey, out string htmlContent))
            {  // Cache not found => Fetch from external source
                Fetch fetcher = new Fetch();

                string url = $"https://parsonious.github.io/CIQ-How-To/pages/{normalizedTitle}.html";
                 htmlContent = await fetcher.FetchAndModifyHtmlAsync(url);
                
                
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
               
           
            return Content(htmlContent, "text/html");
           
        }
    }

}
