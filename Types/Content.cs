using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Services;

namespace ZD_Article_Grabber.Types
{
    public class Content
    {

        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _accessor;
        private readonly IConfigOptions _settings;
        private readonly IResourceFetcher _resourceFetcher;
        internal HtmlDocument HtmlDoc { get; private set; }
        internal ContentNodes Nodes { get; private set; }
        public Content
            (IMemoryCache Cache, 
            IHttpClientFactory ClientFactory, 
            IHttpContextAccessor Accessor, 
            string HtmlContent,
            string sourceUrl,
            IConfigOptions settings,
            IResourceFetcher resourceFetcher)
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
            Nodes = new ContentNodes(HtmlDoc, sourceUrl, settings, resourceFetcher);
        }

        //process files for css, js, sql, ps1 and Images
        public async Task ProcessFilesAsync()
        {
            await Nodes.ProcessNodesAsync(_accessor);
        }

    }
}
