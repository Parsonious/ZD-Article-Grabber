namespace ZD_Article_Grabber.Config
{
    public class FilesConfig
    {
        public required List<string> SupportedFileTypes { get; set; }
        public required Dictionary<string, string> DefaultFiles { get; set; }
    }
}