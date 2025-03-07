using ZD_Article_Grabber.Helpers;

namespace ZD_Article_Grabber.Resources
{
    public sealed record ResourceResult
    {
        public ResourceID Id { get; init; }
        public string Url { get; set; }
        private readonly byte[] _content;
        public ReadOnlySpan<byte> Content => _content;

        public ResourceResult(ResourceID id, byte[] content)
        {
            Id = id;
            _content = content;
        }
    }
}
