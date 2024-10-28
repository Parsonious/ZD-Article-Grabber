using HtmlAgilityPack;

namespace ZD_Article_Grabber.Classes
{
    public class Node
    {
        public HtmlNode HtmlNode { get; private set; }
        public string Xpath { get; private set; }
        public string FileUrl { get; private set; }
        public string Type { get; private set; }
        public string BaseUrl { get; private set; }

        public Node(HtmlNode htmlNode, string baseUrl)
        {
            ArgumentNullException.ThrowIfNull(htmlNode);
            ArgumentNullException.ThrowIfNull(baseUrl);
            Xpath = htmlNode.XPath;
            HtmlNode = htmlNode;
            BaseUrl = baseUrl;
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

            //Resolve the file URL against the base URL
            if(Uri.TryCreate(fileUrl, UriKind.Absolute, out Uri resolvedUri) )
            {
                //file url is already an Absolute Url (i.e. fully qualified)
                return resolvedUri.ToString();
            }
            else
            {
                //Resolve relative URL against baseUrl
                resolvedUri = new Uri(new Uri(BaseUrl), fileUrl);
                return resolvedUri.ToString();
            }
        }

        //determine the type of node (css, js, img) based on tag name and attributes
        private string GetNodeType()
        {
            string tagName = HtmlNode.Name.ToLower(); // Normalize to lowercase for comparison

            if ( tagName == "link" && HtmlNode.GetAttributeValue("rel", "") == "stylesheet" )
                return "css";
            else if ( tagName == "script" )
                return "js";
            else if ( tagName == "img" )
                return "img";
            else if ( tagName == "a" )
            {
                string href = HtmlNode.GetAttributeValue("href", "");
                if ( href.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) )
                    return "sql";
                else if ( href.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) )
                    return "ps1";
            }
            throw new InvalidOperationException($"Unknown type for node: {HtmlNode.Name}");
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
