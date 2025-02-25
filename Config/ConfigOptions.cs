using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Config
{

    public class ConfigOptions : IConfigOptions
    {
        public required PathsConfig Paths { get; set; }
        public required FilesConfig Files { get; set; }
        public required JwtConfig Jwt { get; set; }
        public required DomainClaimsConfig DomainClaims { get; set; }
        public required Dictionary<string, string> XPathQueries { get; set; }
        public required KeyManagementConfig KeyManagement { get; set; }
        public required RefererConfig Referer { get; set; }
    }
}
///TODO Test for the following:
///why the fuck is swagger still asking for an authorization key
///how to fuck do i call this api via curl
///how the fuck do I test this shit in swagger now
