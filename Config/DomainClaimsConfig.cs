namespace ZD_Article_Grabber.Config
{
    public sealed class DomainClaimsConfig
    {
        public required IReadOnlyDictionary<string, DomainClaimsSettings> Settings { get; set; } = new Dictionary<string, DomainClaimsSettings>();
    }
}