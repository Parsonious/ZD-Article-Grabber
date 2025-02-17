using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using HtmlAgilityPack;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Resources.Nodes
{
    public class Node(HtmlNode htmlNode, IConfigOptions settings, IPathHelper pathHelper, IResourceInstructions resourceInstructions)
    {
        public HtmlNode HtmlNode { get; private set; } = htmlNode ?? throw new ArgumentNullException(nameof(htmlNode));
        public NodeContent Content { get; internal set; }
        public NodeID Id { get; private set; } = new NodeID(htmlNode, settings, pathHelper);
        private readonly IResourceInstructions _resourceInstructions = resourceInstructions;    
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public async Task ProcessNodeAsync()
        {
            await UpdateHtmlNode();
        }
        //This handles how the node is "delivered" to the HTML page
        public async Task UpdateHtmlNode()
        {
           if ( _resourceInstructions.Instructions.TryGetValue(Id.Type, out var instruction)) 
           {
                var key = (instruction, Content);
                switch ( key )
                {
                    case (Instructions.PlainText, NodeContentString { Content: var content}): 
                        await HandlePlainText(Id.Type, content);
                        break;
                    case (Instructions.Reference, NodeContent):
                        await HandleReference(Id);
                        break;
                    default:
                        await HandleUnsupported(Id.Type, instruction);
                        break;
                }
                await Task.CompletedTask; //simulate async 
            }
        }
        private async Task HandlePlainText(ResourceType type, string nodeContent)
        {
            await Task.Yield();
            try
            {
                //all current ByteToText types supported
                if ( type is ResourceType.SQL or ResourceType.PS1 or ResourceType.JS or ResourceType.CSS ) 
                {
                    await Task.Run(() => BuildCodeBlock(nodeContent, type));
                }
                else
                {
                    await Task.Run(() => HandleUnsupported(type));
                }
            }
            catch ( Exception ex )
            {
                HtmlNode.InnerHtml = $"<!-- Error: {ex.Message} -->";
            }
        }
        private async Task HandleReference(NodeID id)
        {
            string lockKey = $"{id.ID}-{id.Name}";
            SemaphoreSlim semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                switch ( Id.Type )
                {
                    case (ResourceType.IMG):
                        HtmlNode.Name = "img";
                        HtmlNode.Attributes.RemoveAll();
                        HtmlNode.SetAttributeValue("src", Id.ExternalResourceUrl);
                        break;
                    case (ResourceType.SQL or ResourceType.PS1):
                        HtmlNode.Name = "a";
                        HtmlNode.Attributes.RemoveAll();
                        HtmlNode.SetAttributeValue("href", Id.ExternalResourceUrl);
                        HtmlNode.InnerHtml = Id.Name;
                        break;
                    case (ResourceType.JS):
                        HtmlNode.Name = "script";
                        HtmlNode.Attributes.RemoveAll();
                        HtmlNode.SetAttributeValue("src", Id.ExternalResourceUrl);
                        HtmlNode.SetAttributeValue("type", "text/javascript");
                        HtmlNode.InnerHtml = "";
                        break;
                    case (ResourceType.CSS):
                        HtmlNode.Name = "link";
                        HtmlNode.Attributes.RemoveAll();
                        HtmlNode.SetAttributeValue("rel", "stylesheet");
                        HtmlNode.SetAttributeValue("href", Id.ExternalResourceUrl);
                        break;
                    default:
                        HtmlNode.InnerHtml = $"<!-- Unsupported type: {Id.Type} -->";
                        break;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
        private async Task HandleUnsupported(ResourceType type, Instructions instructions)
        {
            await Task.Yield();
            HtmlNode.InnerHtml = $"<!-- Unsupported type: {type} and Instructions: {instructions} combination -->";

        }
        private async Task HandleUnsupported(ResourceType type)
        {
            await Task.Yield();
            HtmlNode.InnerHtml = $"<!-- Unsupported type: {type} -->";
        }
        private async void BuildCodeBlock(string content, ResourceType resource) 
        {
            string lockKey = $"{Id.ID}-{Id.Name}";
            SemaphoreSlim semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
            
            await semaphore.WaitAsync();

            string type = resource switch
            {
                ResourceType.SQL => "sql",
                ResourceType.PS1 => "powershell",
                ResourceType.CSS => "css",
                ResourceType.JS => "javascript",
                _ => throw new InvalidOperationException($"Unsupported type: {resource}")
            };

            try
            {
                var doc = HtmlNode.OwnerDocument;

                //create wrapper div
                HtmlNode container = doc.CreateElement("div");

                // Create the pre element
                HtmlNode pre = doc.CreateElement("pre");

                //Create copy button
                HtmlNode button = doc.CreateElement("button");
                button.Attributes.Add("id", $"{Id.ID}-btn");
                button.AddClass("copy-code-button");
                button.SetAttributeValue("data-copy-target", $"{Id.ID}-code");
                button.InnerHtml = @"<svg aria-hidden='true' class='copy-icon' Copy </svg>";

                // Create the code element
                HtmlNode code = doc.CreateElement("code");
                code.Attributes.Add("id", $"{Id.ID}-code");
                code.Attributes.Add("class", $"language-{type}");
                code.InnerHtml = System.Net.WebUtility.HtmlEncode(content);

                //build the structure
                pre.AppendChild(button);
                pre.AppendChild(code);

                container.AppendChild(pre);

                HtmlNode.AppendChild(container);
            }
            catch
            {
                HtmlNode.InnerHtml = $"<!-- Error: Failed to build code block for {Id.ID} -->";
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}

