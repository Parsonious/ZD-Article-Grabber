using System.Text;
using HtmlAgilityPack;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Resources.Nodes
{
    public class Node(HtmlNode htmlNode, IConfigOptions settings)
    {
        public HtmlNode HtmlNode { get; private set; } = htmlNode ?? throw new ArgumentNullException(nameof(htmlNode));
        public NodeContent Content { get; internal set; }
        public NodeID Id { get; private set; } = new NodeID(htmlNode, settings);

        public async Task ProcessNodeAsync()
        {
            UpdateHtmlNode();
        }
        public void UpdateHtmlNode()
        {
            var key = (Id.Type, Content);
            switch (key)
            {
                case (ResourceType.Sql or ResourceType.Ps1, NodeContentString { Content: var content }): //sql or ps1 node handled here
                    HtmlNode.RemoveAll();
                    HtmlNode.Name = "div";
                    HtmlNode.InnerHtml = $"<pre><code class=\"language-sql\">{System.Net.WebUtility.HtmlEncode(content)}</code></pre>";
                    break;
                case (ResourceType.Img, NodeContentBytes):
                    HtmlNode.SetAttributeValue(HtmlNode.Name.ToLower() == "link" ? "href" : "src", Id.ResourceUrl);
                    break;
                case (ResourceType.Css, NodeContentString { Content: var content }):
                    HtmlNode.SetAttributeValue("href", Id.ResourceUrl);
                    break;
                case (_, NodeContentBytes b):
                    // Handle 'Other' types or byte[] content
                    HtmlNode.InnerHtml = $"<!-- Unsupported type: {Id.Type} with byte[] content -->";
                    break;
                case (_, NodeContentString s):
                    // Handle 'Other' types or string content
                    HtmlNode.InnerHtml = $"<!-- Unsupported type: {Id.Type} with string content -->";
                    break;
                default:
                    HtmlNode.InnerHtml = $"<!-- Unsupported combination: {Id.Type}, {Content.GetType().Name} -->";
                    break;
            }
        }

    }
}

