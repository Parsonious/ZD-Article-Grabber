using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Controllers
{
    [Route("a/p")]
    [ApiController]
    public class ContentController(IContentFetcher fetchService) : ControllerBase
    {
        private readonly IContentFetcher _fetchService = fetchService;


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetContent()
        {
            string? titleClaim = User.FindFirst("title")?.Value;
            if(string.IsNullOrEmpty(titleClaim) )
            {
                return BadRequest("Invalid token: Missing title claim");
            }

            var htmlContent = await _fetchService.FetchHtmlAsync(titleClaim);
            
            if ( htmlContent == null )
            {
                return NotFound("Page Not Found");
            }
            return Content(htmlContent, "text/html");
        }
    }
}
