
namespace ZD_Article_Grabber.Interfaces
{
    public interface ICacheService
    {
        bool TryGet<TItem>(object key, out TItem value);
        void Set<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
    }
}