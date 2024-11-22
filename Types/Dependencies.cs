using HtmlAgilityPack;
using ZD_Article_Grabber.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using ZD_Article_Grabber.Helpers;

namespace ZD_Article_Grabber.Types
{
    public class Dependencies
    {
        public IHttpClientFactory ClientFactory { get; init; }
        public IHttpContextAccessor Accessor { get; init; }
        public IConfigOptions Settings { get; init; }
        public IMemoryCache Cache { get; init; }
        public IPathHelper PathHelper { get; init; }
        public Dependencies(
        IHttpClientFactory clientFactory,
        IHttpContextAccessor accessor,
        IConfigOptions settings,
        IMemoryCache cache,
        IPathHelper pathHelper)
        {
            ArgumentNullException.ThrowIfNull(clientFactory, nameof(clientFactory));
            ArgumentNullException.ThrowIfNull(accessor, nameof(accessor));
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(cache, nameof(cache));
            ArgumentNullException.ThrowIfNull(pathHelper, nameof(pathHelper));

            ClientFactory = clientFactory;
            Accessor = accessor;
            Settings = settings;
            Cache = cache;
            PathHelper = pathHelper;
        }
    }
}