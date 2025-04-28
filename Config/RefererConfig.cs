using System.Collections.Immutable;

namespace ZD_Article_Grabber.Config
{
    public sealed class RefererConfig
    {
        public ImmutableArray<string> _allowedDomains { get; init; } = ImmutableArray<string>.Empty;
        public string[] AllowedDomains
        {
            get => _allowedDomains.ToArray();
            init => _allowedDomains = value != null ? ImmutableArray.Create(value) : ImmutableArray<string>.Empty;
        }
        public ImmutableArray<string> AllowedDomainsIm => _allowedDomains;
        public required bool EnforceHttps { get; init; }
    }
}
