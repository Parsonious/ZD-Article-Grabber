using HtmlAgilityPack;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Types
{
    public class Node
    {
        public HtmlNode HtmlNode { get; private set; }
        public string Xpath { get; private set; }
        public string FileUrl { get; private set; }
        public string FileName { get; private set; }
        public string Type { get; private set; }

        public Node(HtmlNode htmlNode, string baseUrl, IConfigOptions Settings)
        {
            HtmlNode = htmlNode ?? throw new ArgumentNullException(nameof(htmlNode));
            Xpath = htmlNode.XPath;
            Type = GetNodeType();
            FileUrl = GetFileUrl(baseUrl);
            FileName = Path.GetFileName(new Uri(FileUrl).LocalPath);
        }
        private string GetFileUrl(string baseUrl)
        {
            var fileUrl = HtmlNode.GetAttributeValue("href", null) ?? HtmlNode.GetAttributeValue("src", string.Empty);

            if ( string.IsNullOrEmpty(fileUrl) )
            {
                throw new ArgumentException("FileUrl cannot be empty");
            }

            if ( !Uri.IsWellFormedUriString(fileUrl, UriKind.Absolute) )
            {
                fileUrl = new Uri(new Uri(baseUrl), fileUrl).ToString();
            }

            return fileUrl;
        }

        private string GetNodeType()
        {
            string tagName = HtmlNode.Name.ToLower();

            return tagName switch
            {
                "link" when HtmlNode.GetAttributeValue("rel", "") == "stylesheet" => "css",
                "script" => "js",
                "img" => "img",
                "a" when HtmlNode.GetAttributeValue("href", "").EndsWith(".sql", StringComparison.OrdinalIgnoreCase) => "sql",
                "a" when HtmlNode.GetAttributeValue("href", "").EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) => "ps1",
                _ => throw new InvalidOperationException($"Unknown type for node: {HtmlNode.Name}")
            };
        }

        public void SetLocalPath(string localPath)
        {
            switch ( Type )
            {
                case "css":
                    HtmlNode.SetAttributeValue("href", localPath);
                    break;
                default:
                    HtmlNode.SetAttributeValue("src", localPath);
                    break;
            }
        }
    }
}
