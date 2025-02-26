namespace ZD_Article_Grabber.Config
{
    public readonly record struct ClaimsConfig
    {
        public required string Type { get; init; } = string.Empty;
        public required string Value { get; init; } = string.Empty;

        public ClaimsConfig()
        {
        }
    }
}
