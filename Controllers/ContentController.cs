using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace ZD_Article_Grabber
{
    [Route("a/p")]
    [ApiController]
    public class ContentController(IMemoryCache cache, Fetch fetchService) : ControllerBase //primary constructor used
    {
        private readonly IMemoryCache _cache = cache;
        private readonly Fetch _fetchService = fetchService;


        [HttpGet("{title}")]
        public async Task<IActionResult> GetContent(string title)
        {
            var htmlContent = await _fetchService.FetchHtmlAsync(title);
            if ( htmlContent == null )
            {
                return NotFound("Page Not Found");
            }
            return Content(htmlContent, "text/html");
        }
    }
}
