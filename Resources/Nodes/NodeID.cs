
using System.Reflection;
using HtmlAgilityPack;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Resources.Nodes
{

    public class NodeID : ResourceID
    {
        public string Xpath { get; private set; }
        private readonly HtmlNode _htmlNode;
        private readonly IConfigOptions _settings;
        public static readonly HashSet<string> PathAttributes = new(StringComparer.OrdinalIgnoreCase)

    {
        "href",
        "src",
        "action",
        "data-src",
        "data-href",
        "poster",
        "cite",
        "background",
        "usemap"
    };
        public NodeID(HtmlNode htmlNode, IConfigOptions settings)
        {
            _htmlNode = htmlNode;
            _settings = settings;
            Type = GetResourceType();
            LocalUrl = CompletePath(GetResourcePath(htmlNode), Type);
            RemoteUrl = GetFileUrl(settings.Paths.UrlPath);
            Name = Path.GetFileName(new Uri(RemoteUrl).LocalPath);
            Xpath = htmlNode.XPath;
            GenerateCacheKey();
        }

        public ResourceType GetResourceType()
        {
            string tagName = _htmlNode.Name.ToLower();

            return tagName switch
            {
                "link" when _htmlNode.GetAttributeValue("rel", "") == "stylesheet" => ResourceType.Css,
                "script" => ResourceType.Js,
                "img" => ResourceType.Img,
                "a" when _htmlNode.GetAttributeValue("href", "").EndsWith(".sql", StringComparison.OrdinalIgnoreCase) => ResourceType.Sql,
                "a" when _htmlNode.GetAttributeValue("href", "").EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) => ResourceType.Ps1,
                _ => throw new InvalidOperationException($"Unknown type for node: {_htmlNode.Name}")
            };
        }
        public string GetResourcePath(HtmlNode node)
        {
            foreach (var attribute in PathAttributes)
            {
                var attrValue = node.GetAttributeValue(attribute, null);
                if (!string.IsNullOrEmpty(attrValue))
                {
                    return attrValue;
                }
            }
            //return a bobo path if no attribute matches 
            return _settings.Paths.ResourceFilesPath;
        }
        public string CompletePath(string extractedPath, ResourceType type)
        {
            //remove any .. from the path
            while (extractedPath.StartsWith(".."))
            {
                extractedPath = extractedPath[2..]; //WAN for .Substring(2);
            }

            //set OS specific path separators
            extractedPath = extractedPath.Replace('\\', '/').TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            //finalize the path by combining the resource path with the extracted path
            return Path.Combine(_settings.Paths.ResourceFilesPath, type.ToString().ToLower(), extractedPath);
        }
        public string GetFileUrl(string baseUrl)
        {
            var fileUrl = _htmlNode.GetAttributeValue("href", null) ?? _htmlNode.GetAttributeValue("src", string.Empty);

            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("FileUrl cannot be empty");
            }

            if (!Uri.IsWellFormedUriString(fileUrl, UriKind.Absolute))
            {
                // TODO: sanitize input here so it matches against what is expected.
                /*Issue #12 created for this bug.*/
                fileUrl = new Uri(new Uri(baseUrl), fileUrl).ToString(); //yeah this is taking the file url and forming it into a busted web url
                //need to either sanitize the input again here or pass the pre url sanitized input to this method call (spaces and such are getting throug here)
            }

            return fileUrl;
        }
    }
}