using HtmlAgilityPack;
using System.Net.Http;
using System.IO;

namespace ZD_Article_Grabber
{
    public class Fetch
    {
        public async Task<string> FetchAndModifyHtmlAsync(string url)
        {
            using ( var client = new HttpClient() )
            {
                //Fetch the HTML content
                var htmlContent = await client.GetStringAsync(url);

                //load the HTML into HAP
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                //identify and process stylesheets
                foreach ( var link in htmlDoc.DocumentNode.SelectNodes("//link[@rel='stylesheet']") )
                {
                    var cssUrl = link.GetAttributeValue("href", string.Empty);
                    if ( !string.IsNullOrEmpty(cssUrl) )
                    {
                        //handle css processing
                        var localCssPath = await ProcessCssOrJsFile(cssUrl, "css", client, url);
                        link.SetAttributeValue("href", localCssPath);
                    }
                }

                //Identify and process JS files
                foreach ( var script in htmlDoc.DocumentNode.SelectNodes("//script[@src]") )
                {
                    var jsUrl = script.GetAttributeValue("src", string.Empty);
                    if ( !string.IsNullOrEmpty(jsUrl) )
                    {
                        //Handle JS processing
                        var localJsPath = await ProcessCssOrJsFile(jsUrl, "js", client, url);
                        script.SetAttributeValue("src", localJsPath);
                    }
                }
                return htmlDoc.DocumentNode.OuterHtml;
            }

        }
        private static async Task<string> ProcessCssOrJsFile(string fileUrl, string fileType, HttpClient client, string baseUrl)
        {
            // Resolve the full URL if it's relative
            var resolvedUrl = new Uri(new Uri(baseUrl), fileUrl).ToString();
            var fileName = Path.GetFileName(resolvedUrl);

            // Define the local file path
            var tempDirectory = Path.Combine("wwwroot", "static", fileType);
            Directory.CreateDirectory(tempDirectory); // Ensure directory exists
            var localFilePath = Path.Combine(tempDirectory, fileName);

            // Check if the file already exists locally
            if ( !File.Exists(localFilePath) )
            {
                // Download and save the file
                var fileContent = await client.GetByteArrayAsync(resolvedUrl);
                await File.WriteAllBytesAsync(localFilePath, fileContent);
            }

            // Return the local path to be used in the HTML
            return $"/static/{fileType}/{fileName}";
        }
    }
}
