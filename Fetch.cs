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

                // Select both <link> and <script> elements that we want to processs
                var nodes = htmlDoc.DocumentNode.SelectNodes("//link[@rel='stylesheet'] | //script[@src]");

                foreach ( var node in nodes )
                {
                    string fileUrl = string.Empty;
                    string fileType = string.Empty;
                    switch (node.Name)
                    {
                        case "link":
                            // Process stylesheets
                            fileUrl = node.GetAttributeValue("href", string.Empty);
                            fileType = "css";
                            break;
                        case "script":
                            // Process JavaScript files
                            fileUrl = node.GetAttributeValue("src", string.Empty);
                            fileType = "js";
                            break;
                    }

                    if ( !string.IsNullOrEmpty(fileUrl) )
                    {
                        // Handle file processing (CSS or JS)
                        var localPath = await ProcessCssOrJsFile(fileUrl, fileType, client, url);
                        if ( fileType == "css" )
                        {
                            node.SetAttributeValue("href", localPath);
                        }
                        else if ( fileType == "js" )
                        {
                            node.SetAttributeValue("src", localPath);
                        }
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
