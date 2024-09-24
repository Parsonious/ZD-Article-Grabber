using HtmlAgilityPack;

namespace ZD_Article_Grabber.Classes
{
    public class Node
    {
        public HtmlNode HtmlNode { get; private set; }
        public string Xpath { get; private set; }
        public string FileUrl { get; private set; }
        public string Type { get; private set; }
        public string Content { get; private set; }

        public Node(HtmlNode htmlNode)
        {
            ArgumentNullException.ThrowIfNull(htmlNode);
            Xpath = htmlNode.XPath;
            HtmlNode = htmlNode;
            FileUrl = GetFileUrl();
            Type = GetNodeType();
        }
        private string GetFileUrl()
        {
            var fileUrl = HtmlNode.GetAttributeValue("href", null) ?? HtmlNode.GetAttributeValue("src", string.Empty);

            //validate
            if ( string.IsNullOrEmpty(fileUrl) )
            {
                throw new ArgumentException("FileUrl cannot be empty");
            }
            return fileUrl;
        }

        //determine the type of node (css, js, img) based on tag name and attributes
        private string GetNodeType()
        {
            string tagName = HtmlNode.Name.ToLower(); // Normalize to lowercase for comparison

            return tagName switch
            {
                "link" when HtmlNode.GetAttributeValue("rel", "") == "stylesheet" => "css",
                "script" => "js",
                "img" => "img",
                _ => throw new InvalidOperationException($"Unknown type for node: {HtmlNode.Name}")
            };
        }
        //set the local path for the node 
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
