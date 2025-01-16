using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Resources.Pages
{
    public class PageID : ResourceID
    {
        public IConfigOptions Config { get; init; }
        public IPathHelper PathHelper { get; init; }
        public PageID(string title, IConfigOptions config, IPathHelper pathHelper)
        {
            Type = ResourceType.Html;
            Config = config;
            PathHelper = pathHelper;

            Name = PathHelper.NormalizeTitle(title); //sanitize title and clear user added extension
            Name = Path.ChangeExtension(Name, PathHelper.GetExtension(Type)); //add back necessary extension based on ResourceType

            LocalUrl = Path.Combine(Config.Paths.HtmlFilesPath, Name);
            ExternalResourceUrl = $"{Config.Paths.ExternalResourceUrlPath}{Uri.EscapeDataString(Name)}"; //make normalized name url safe
            FallbackRemoteUrl = $"{Config.Paths.FallbackRemoteUrlPath}{Uri.EscapeDataString(Name)}"; //make normalized name url safe
            GenerateCacheKey();
        }

    }
}