using System.Text;
using HtmlAgilityPack;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Resources.Nodes
{
    public class Node(HtmlNode htmlNode, IConfigOptions settings, IPathHelper pathHelper)
    {
        public HtmlNode HtmlNode { get; private set; } = htmlNode ?? throw new ArgumentNullException(nameof(htmlNode));
        public NodeContent Content { get; internal set; }
        public NodeID Id { get; private set; } = new NodeID(htmlNode, settings, pathHelper);

        public async Task ProcessNodeAsync()
        {
            await UpdateHtmlNode();
        }
        public async Task UpdateHtmlNode()
        {
            var key = (Id.Type, Content);
            switch (key)
            {
                case (ResourceType.Sql or ResourceType.Ps1, NodeContentString { Content: var content }): //sql or ps1 node handled here
                    HtmlNode.RemoveAll();
                    HtmlNode.Name = "div";
                    HtmlNode.InnerHtml = $"<pre id=\"{Id.ID}\" ><button id=\"{Id.ID}\" class=\"copy-code-button\">Copy</button><code class=\"language-sql\">{System.Net.WebUtility.HtmlEncode(content)}</code></pre>";
                    break;
                case (ResourceType.Img, NodeContentBytes):
                    HtmlNode.SetAttributeValue("src", Id.ResourceUrl);
                    break;
                case (ResourceType.Css, NodeContentString { Content: var content }): //content is here incase it is ever needed to be embedded in the page
                    HtmlNode.Name = "link";
                    HtmlNode.Attributes.RemoveAll();
                    HtmlNode.SetAttributeValue("rel", "stylesheet");
                    HtmlNode.SetAttributeValue("href", Id.ResourceUrl);
                    break;
                case (ResourceType.Js, NodeContentString):
                    HtmlNode.Name = "script";
                    HtmlNode.Attributes.RemoveAll();
                    HtmlNode.SetAttributeValue("src", Id.ResourceUrl);
                    HtmlNode.InnerHtml = ""; //clear any inline scripting
                    break;
                case (_, NodeContentBytes):
                    // Handle 'Other' types or byte[] content
                    HtmlNode.InnerHtml = $"<!-- Unsupported type: {Id.Type} with byte[] content -->";
                    break;
                case (_, NodeContentString):
                    // Handle 'Other' types or string content
                    HtmlNode.InnerHtml = $"<!-- Unsupported type: {Id.Type} with string content -->";
                    break;
                default:
                    HtmlNode.InnerHtml = $"<!-- Unsupported combination: {Id.Type}, {Content.GetType().Name} -->";
                    break;
            }
            await Task.CompletedTask; //simulate async 
        }

    }
}

