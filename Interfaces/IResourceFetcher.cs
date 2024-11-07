namespace ZD_Article_Grabber.Interfaces
{
    public interface IResourceFetcher
    {
        Task<byte[]> FetchResourceAsync(string fileType, string fileName, string remoteUrl);
    }
}
