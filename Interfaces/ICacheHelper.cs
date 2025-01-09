namespace ZD_Article_Grabber.Interfaces
{
    public interface ICacheHelper
    {
        public string GenerateCacheKey(string prefix, string input);
    }
}
