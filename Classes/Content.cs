using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

namespace ZD_Article_Grabber.Classes
{
    public class Content
    {

        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _accessor;
        
        private readonly Dictionary<string, string> _xpathDictionary = new Dictionary<string, string>()
        {
            { "//link[@rel='stylesheet' and @href]", "css" },
            { "//script[@src]", "js" },
            { "//img[@src]", "img" }
        };
        internal HtmlDocument HtmlDoc { get; private set; }
        internal ContentNodes Nodes { get; private set; }
        public Content(IMemoryCache Cache, IHttpClientFactory ClientFactory, IHttpContextAccessor Accessor, string HtmlContent, string sourceUrl)
        {
            ArgumentNullException.ThrowIfNull(Cache, nameof(Cache));
            ArgumentNullException.ThrowIfNull(ClientFactory, nameof(ClientFactory));
            ArgumentNullException.ThrowIfNull(Accessor, nameof(Accessor));
            ArgumentException.ThrowIfNullOrWhiteSpace(HtmlContent, nameof(HtmlContent));
            _cache = Cache;
            _clientFactory = ClientFactory;
            _accessor = Accessor;

            //Initialize the HTML Doc and ContentNodes
            HtmlDoc = new HtmlDocument();
            HtmlDoc.LoadHtml(HtmlContent);
            Nodes = new ContentNodes(HtmlDoc, _xpathDictionary, sourceUrl);
        }

        //process files for css, js, and Images
        public async Task ProcessFilesAsync()
        {
            await Nodes.ProcessNodesAsync(_cache,_accessor,_clientFactory);
        }

    }
}
