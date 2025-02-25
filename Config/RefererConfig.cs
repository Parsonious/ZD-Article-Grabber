namespace ZD_Article_Grabber.Config
{
    public class RefererConfig
    {
        public required List<string> AllowedDomains { get; set; }
        public required bool EnforceHttps { get; set; }
    }
}
