using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;
using HtmlAgilityPack;
using ZD_Article_Grabber.Resources.Nodes;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Resources.Pages
{
    public class Page(PageID id, string html, Dependencies dependencies, INodeBuilder nodeBuilder)
    {
        public PageID Id { get; init; } = id;
        public string Html { get; set; } = html;
        public List<Node> Nodes { get; set; } = []; //wild ass notation (WAN) for new List<Node>();
        private readonly Dependencies _dependencies = dependencies;
        private readonly INodeBuilder _nodeBuilder = nodeBuilder;

        private async Task<List<Node>> InitializeNodes()
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(Html);

            var htmlNodes = htmlDoc.DocumentNode
                                    .SelectNodes(string.Join("|", _dependencies.Settings.XPathQueries.Keys))
                                    ?.ToList() ?? []; //wild ass notation for: new List<HtmlNode>();

            if (htmlNodes.Count > 5)
            {
                //process in parallel
                return await Task.WhenAll(htmlNodes.Select(node => _nodeBuilder.BuildNodeAsync(node))).ContinueWith(t => t.Result.ToList());
            }
            //process sequentially for simple pages
            List<Node> nodes = new(htmlNodes.Count);
            foreach ( var node in htmlNodes)
            {
                nodes.Add(await _nodeBuilder.BuildNodeAsync(node));
            }
            return nodes;

        }
        public async Task ProcessFilesAsync()
        {
            Nodes = await InitializeNodes();

            IEnumerable<Task> nodeProcessTasks = Nodes.Select(async node =>
            {
                await node.ProcessNodeAsync();
            });
            await Task.WhenAll(nodeProcessTasks);
            FinalizeHtml();
        }
        public void FinalizeHtml()
        {
            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(Html);

            foreach (Node node in Nodes)
            {
                var originalNode = htmlDocument.DocumentNode.SelectSingleNode(node.Id.Xpath);
                originalNode?.ParentNode.ReplaceChild(node.HtmlNode, originalNode);//if node is not null replacechild..called null propogation
            }

            // Set the updated HTML content to the Html property
            Html = htmlDocument.DocumentNode.OuterHtml;
        }
    }
}
