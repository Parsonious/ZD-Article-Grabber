using ZD_Article_Grabber.Resources.Pages;

namespace ZD_Article_Grabber.Resources.Cache;
    public class CachedPage(Page page, DateTimeOffset expiration)
    {
        public Page Page { get; init; } = page;
        public DateTimeOffset Expiration { get; set; } = expiration;
    }

