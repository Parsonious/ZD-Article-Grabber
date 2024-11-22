using ZD_Article_Grabber.Helpers;

namespace ZD_Article_Grabber.Resources
{
    public record ResourceResult
    {
        public ResourceID Id { get; init; }
        public string Url { get; set; }
        public byte[] Content { get; set; }

        public ResourceResult(ResourceID id, byte[] content)
        {
            Id = id;
            Content = content;
        }
    }
}
