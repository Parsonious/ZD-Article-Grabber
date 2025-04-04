namespace ZD_Article_Grabber.Interfaces
{
    public interface IKeyHistoryService
    {
        void TrackKeyUsage(string key);
        ValueTask<bool> IsKeyValidAsync(string key);
        void Dispose();
    }
}