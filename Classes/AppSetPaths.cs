namespace ZD_Article_Grabber.Classes
{
    public class AppSetPaths
    {
        public string HtmlFilesPath { get; set; }
        public string ResourceFilesPath { get; set; }
        public string UrlPath { get; set; }
        public Dictionary<string, string> DefaultFiles { get; set; }
        public List<string> SupportedFileTypes { get; set; }
    }
}
