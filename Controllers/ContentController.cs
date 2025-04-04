using System.Text;
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
        private static readonly HashSet<string> _compressibleTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "text/html",
            "text/css",
            "text/javascript",
            "application/javascript",
        };


        [HttpGet]
        [Authorize]
        [ResponseCache(Duration = 600)] //10 mins
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
            // Generate ETag from content
            var etag = $"\"{Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(htmlContent))).Substring(0, 16)}\"";

            // Check if client has valid cached version
            var clientETag = Request.Headers.IfNoneMatch.FirstOrDefault();
            if ( clientETag == etag )
            {
                return StatusCode(304); // Not Modified
            }

            Response.Headers.Append("Vary", new[] { "Accept-Encoding" });
            Response.Headers.CacheControl = "public, max-age=600";
            Response.Headers.Append("X-Content-Type-Options", "nosniff");
            Response.Headers.ETag = etag;

            return Content(htmlContent, "text/html; charset=utf-8");
        }
    }
}
