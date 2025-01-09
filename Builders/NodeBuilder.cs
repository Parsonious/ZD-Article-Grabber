using ZD_Article_Grabber.Interfaces;
using HtmlAgilityPack;
using System.Text;
using ZD_Article_Grabber.Resources.Nodes;
using ZD_Article_Grabber.Resources;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Builders
{
    public class NodeBuilder(Dependencies dependencies, IResourceFetcher resourceFetcher) : INodeBuilder
    {
        readonly Dependencies _dependencies = dependencies;
        readonly IResourceFetcher _resourceFetcher = resourceFetcher;
        public async Task<Node> BuildNodeAsync(HtmlNode htmlNode)
        {
            Node node = new(htmlNode, _dependencies.Settings);
            await FetchContentAsync(node);
            return node;
        }
        private async Task FetchContentAsync(Node node)
        {
            switch (node.Id.Type)
            {
                case ResourceType.Img:
                    await HandleByteType(node);
                    break;
                case ResourceType.Css:
                case ResourceType.Sql:
                case ResourceType.Ps1:
                case ResourceType:Js:
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