using ZD_Article_Grabber.Resources;

namespace ZD_Article_Grabber.Interfaces
{
    public interface IResourceFetcher
    {
        Task<ResourceResult> FetchResourceAsync(ResourceID iD);
    }
}
