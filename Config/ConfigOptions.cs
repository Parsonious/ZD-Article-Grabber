using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Config
{

    public sealed class ConfigOptions : IConfigOptions
    {
        public required PathsConfig Paths { get; init; }
        public required FilesConfig Files { get; init; }
        public required JwtConfig Jwt { get; init; }
        public required DomainClaimsConfig DomainClaims { get; init; }
        public required Dictionary<string, string> XPathQueries { get; init; }
        public required KeyManagementConfig KeyManagement { get; init; }
        public required RefererConfig Referer { get; init; }
    }
}

