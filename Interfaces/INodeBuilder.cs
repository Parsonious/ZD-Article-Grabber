using HtmlAgilityPack;
using System.Threading.Tasks;
using ZD_Article_Grabber.Resources.Nodes;

namespace ZD_Article_Grabber.Interfaces
{
    public interface INodeBuilder
    {
        Task<Node> BuildNodeAsync(HtmlNode htmlNode);
    }
}