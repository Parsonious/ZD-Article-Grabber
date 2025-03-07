
using System.Reflection;
using System.Text;
using HtmlAgilityPack;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Resources.Nodes
{

    public class NodeID : ResourceID
    {
        public string Xpath { get; private set; }
        private readonly HtmlNode _htmlNode;
        private readonly IConfigOptions _settings;
        private readonly IPathHelper _pathHelper;
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
        "script",
        "usemap"
    };
        public NodeID(HtmlNode htmlNode, IConfigOptions settings, IPathHelper pathHelper)
        {
            _htmlNode = htmlNode;
            _settings = settings;
            _pathHelper = pathHelper;
            Type = GetResourceType();
            LocalUrl = ConstructLocalUrl(htmlNode,Type);
            FallbackRemoteUrl = ConstructFallBackUrl(settings.Paths.FallbackRemoteUrlPath);
            Name = Path.GetFileName(new Uri(FallbackRemoteUrl).LocalPath);
            ExternalResourceUrl = ConstructExternalResourceUrl(Name);
            Xpath = htmlNode.XPath;
            ID = GetID();
            GenerateCacheKey();
        }

        public ResourceType GetResourceType()
        {
            string tagName = _htmlNode.Name.ToLower();

            return tagName switch
            {
                "link" when _htmlNode.GetAttributeValue("rel", "") == "stylesheet" => ResourceType.CSS,
                "script" => ResourceType.JS,
                "img" => ResourceType.IMG,
                "a" when _htmlNode.GetAttributeValue("href", "").EndsWith(".sql", StringComparison.OrdinalIgnoreCase) => ResourceType.SQL,
                "a" when _htmlNode.GetAttributeValue("href", "").EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) => ResourceType.PS1,
                _ => throw new InvalidOperationException($"Unknown type for node: {_htmlNode.Name}")
            };
        }

        public string ConstructLocalUrl(HtmlNode node, ResourceType type)
        {
            //get relative path from node
            string resourcePath = GetResourcePath(node);
            //GetResourcePath can return a bobo value, check for this
            if ( !string.Equals(resourcePath, _settings.Paths.ResourceFilesPath, StringComparison.OrdinalIgnoreCase) )
            {
                //construct absolute path
                return _pathHelper.CompleteLocalPath(_settings.Paths.ResourceFilesPath, resourcePath, type);
            }
            ///TODO: Handle this possibility better.
            return string.Empty; //return an empty string if bobo path existed can modify this to something else later

        }

       
        public string ConstructFallBackUrl(string baseUrl)
        {
            var fileUrl = _htmlNode.GetAttributeValue("href", null) ?? _htmlNode.GetAttributeValue("src", string.Empty);

            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("FileUrl cannot be empty");
            }

            if (!Uri.IsWellFormedUriString(fileUrl, UriKind.Absolute))
            {
                fileUrl.Replace("\\\\", "/");
                fileUrl = Uri.UnescapeDataString(fileUrl);
                fileUrl = new Uri(new Uri(baseUrl), fileUrl).ToString();
            }

            return fileUrl;
        }
        public string ConstructExternalResourceUrl(string localUrl)
        {
                string typeString = Type.ToString().ToLower();
                string sanitizedPath = _pathHelper.EncodeUrl(localUrl);
                return $"{_settings.Paths.ExternalResourceUrlPath.TrimEnd('/')}/{typeString}/{sanitizedPath}";
        }

        //keeping this inside NodeID to remove what would create an added dependency on HtmlAgilityPack 
        private string GetResourcePath(HtmlNode node)
        {
            foreach ( var attribute in PathAttributes )
            {
                var attrValue = node.GetAttributeValue(attribute, null);
                if ( !string.IsNullOrEmpty(attrValue) )
                {
                    return attrValue;
                }
            }
            //return a bobo path if no attribute matches 
            return _settings.Paths.ResourceFilesPath;
        }

    }
}