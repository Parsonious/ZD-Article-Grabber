using ZD_Article_Grabber.Interfaces;
using HtmlAgilityPack;
using System.Text;
using ZD_Article_Grabber.Resources.Nodes;
using ZD_Article_Grabber.Types;
using System.Threading;
namespace ZD_Article_Grabber.Builders
{
    public class NodeBuilder(Dependencies dependencies, 
        IResourceFetcher resourceFetcher,
        IPathHelper pathHelper,
        IResourceInstructions resourceInstructions) : INodeBuilder
    {
        readonly Dependencies _dependencies = dependencies;
        readonly IResourceFetcher _resourceFetcher = resourceFetcher;
        private readonly IResourceInstructions _resourceInstructions = resourceInstructions;
        public async Task<Node> BuildNodeAsync(HtmlNode htmlNode)
        {
            Node node = new(htmlNode, _dependencies.Settings, pathHelper, _resourceInstructions);
            await FetchContentAsync(node);
            return node;
        }
        //This is where the node is actually built based on resource type
        private async Task FetchContentAsync(Node node)
        {
            switch (node.Id.Type)
            {
                case ResourceType.IMG:
                    await HandleByteType(node);
                    break;
                case ResourceType.CSS:
                case ResourceType.SQL:
                case ResourceType.PS1:
                case ResourceType:JS:
                    await ByteToTextType(node);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported node type: {node.Id.Type}");
            }
        }
        private async Task ByteToTextType(Node node)
        {
            var resource = await _resourceFetcher.FetchResourceAsync(node.Id);
            string contentString = Encoding.UTF8.GetString(resource.Content);
            node.Content = new NodeContentString(contentString);
            node.Id.ResourceUrl = resource.Url;
        }
        private async Task HandleByteType(Node node)
        {
            var resource = await _resourceFetcher.FetchResourceAsync(node.Id);
            node.Content = new NodeContentBytes(resource.Content);
            node.Id.ResourceUrl = resource.Url;
        }

    }
}