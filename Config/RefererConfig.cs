using System.Collections.Immutable;

namespace ZD_Article_Grabber.Config
{
    public sealed class RefererConfig
    {
        public required ImmutableArray<string> AllowedDomains { get; init; }
        public required bool EnforceHttps { get; init; }
    }
}
