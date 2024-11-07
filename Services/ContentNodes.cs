using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Services
{
    public class ContentNodes
    {
        public List<Node> Nodes { get; private set; }
        internal IConfigOptions Settings { get; private set; }
        internal IResourceFetcher _resourceFetcher { get; private set; }

        public ContentNodes(HtmlDocument doc, string baseUrl, IConfigOptions settings, IResourceFetcher resourceFetcher)
        {
            ArgumentNullException.ThrowIfNull(doc,nameof(doc));
            ArgumentNullException.ThrowIfNull(settings,nameof(settings));

            Settings = settings;
            _resourceFetcher = resourceFetcher;

            Nodes = doc.DocumentNode.SelectNodes(string.Join("|", Settings.XPathQueries.Keys))      //get xpath search text from xpathDictionary
                    ?.Select(htmlNode => new Node(htmlNode, baseUrl, settings)).ToList()                     //get HtmlNode info from Node type
                    ?? new List<Node>();
        }

        //process and apply the local paths in parallel
        public async Task ProcessNodesAsync(IHttpContextAccessor accessor)
        {
            var scheme = accessor.HttpContext.Request.Scheme;
            var host = accessor.HttpContext.Request.Host.Value;

            var tasks = Nodes.Select(async node =>
            {
                try
                {
                    string localPath = await ProcessNodeAsync(_resourceFetcher, scheme, host, node);
                    node.SetLocalPath(localPath);
                }
                catch ( Exception )
                {
                    string defaultPath = $"{scheme}://{host}/a/c/{node.Type}/default";
                    node.SetLocalPath(defaultPath);
                }
            });

            await Task.WhenAll(tasks);
        }
        private async Task<string> ProcessNodeAsync(IResourceFetcher resourceFetcher, string scheme, string host, Node node)
        {
            var fileType = node.Type;
            var fileName = node.FileName;
            var fileUrl = node.FileUrl;

            // Fetch the resource using ResourceFetcher
            await resourceFetcher.FetchResourceAsync(fileType, fileName, fileUrl);

            return $"{scheme}://{host}/a/c/{fileType}/{Uri.EscapeDataString(fileName)}";
        }
    }
}
