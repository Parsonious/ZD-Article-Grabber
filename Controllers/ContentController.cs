using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Controllers
{
    [Route("a/p")]
    [ApiController]
    public class ContentController(IContentFetcher fetchService) : ControllerBase
    {
        private readonly IContentFetcher _fetchService = fetchService;


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
