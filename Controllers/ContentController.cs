using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ContentController : ControllerBase
{
    [HttpGet("{title}")]
    public IActionResult GetContent(string title)
    {
        // Normalize title to match your file naming convention
        string normalizedTitle = title.Replace(" ", "%20");

        // Construct the URL
        string url = $"https://parsonious.github.io/CIQ-How-To/pages/{normalizedTitle}.html";

        using ( var client = new HttpClient() )
        {
            var response = client.GetAsync(url).Result;
            if ( response.IsSuccessStatusCode )
            {
                string htmlContent = response.Content.ReadAsStringAsync().Result;
                return Content(htmlContent, "text/html");
            }
            else
            {
                return NotFound("Page not found");
            }
        }
    }
}
