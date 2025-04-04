namespace ZD_Article_Grabber.Config
{
    public sealed class FilesConfig
    {
        public required IReadOnlyList<string> SupportedFileTypes { get; init; }
        public required IReadOnlyDictionary<string, string> DefaultFiles { get; init; }
    }
}