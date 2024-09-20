using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

namespace ZD_Article_Grabber.Classes
{
    public class Content
    {

        private readonly IMemoryCache _cache;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _accessor;
        
        private readonly Dictionary<string, string> _xpathDictionary = new Dictionary<string, string>()
        {
            { "//link[@rel='stylesheet']", "css" },
            { "//script[@src]", "js" },
            { "//img[@src]", "img" }
        };
        internal HtmlDocument HtmlDoc { get; private set; }
        internal ContentNodes Nodes { get; private set; }
        public Content(IMemoryCache Cache, HttpClient Client, IHttpContextAccessor Accessor, string HtmlContent)
        {
            ArgumentNullException.ThrowIfNull(Cache, nameof(Cache));
            ArgumentNullException.ThrowIfNull(Client, nameof(Client));
            ArgumentNullException.ThrowIfNull(Accessor, nameof(Accessor));
            ArgumentException.ThrowIfNullOrWhiteSpace(HtmlContent, nameof(HtmlContent));
            _cache = Cache;
            _client = Client;
            _accessor = Accessor;

            //Initialize the HTML Doc and ContentNodes
            HtmlDoc = new HtmlDocument();
            HtmlDoc.LoadHtml(HtmlContent);
            Nodes = new ContentNodes(HtmlDoc, _xpathDictionary);
        }

        //process files for css, js, and Images
        public async Task ProcessFilesAsync()
        {
            await Nodes.ProcessNodesAsync(_client,_cache,_accessor);
        }

    }
}
