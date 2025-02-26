namespace ZD_Article_Grabber.Config
{
    public readonly record struct PathsConfig
    {
        public required string HtmlFilesPath { get; init; }
        public required string ResourceFilesPath { get; init; }
        public required string FallbackRemoteUrlPath { get; init; }
        public required string ExternalResourceUrlPath { get; init; }

    }
}