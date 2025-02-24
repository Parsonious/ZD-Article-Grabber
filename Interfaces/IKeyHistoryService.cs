namespace ZD_Article_Grabber.Interfaces
{
    public interface IKeyHistoryService
    {
        void TrackKeyUsage(string key);
        Task<bool> IsKeyValidAsync(string key);
        void Dispose();
    }
}