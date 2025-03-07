namespace ZD_Article_Grabber.Config
{
    public sealed class DomainClaimsSettings
    {
        public required IReadOnlyList<ClaimsConfig> Claims { get; set; } = [];
    }
}
