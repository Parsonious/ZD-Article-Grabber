using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Types
{
    public class ConfigOptions : IConfigOptions
    {
        public PathsOptions Paths { get; set; }
        public FilesOptions Files { get; set; }
        public Dictionary<string, string> XPathQueries { get; set; }
    }

    public class PathsOptions
    {
        public string HtmlFilesPath { get; set; }
        public string ResourceFilesPath { get; set; }
        public string UrlPath { get; set; }
    }

    public class FilesOptions
    {
        public List<string> SupportedFileTypes { get; set; }
        public Dictionary<string, string> DefaultFiles { get; set; }
    }
}
