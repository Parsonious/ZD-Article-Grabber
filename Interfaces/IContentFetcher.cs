namespace ZD_Article_Grabber.Interfaces
{
       public interface IContentFetcher
    {
        Task<string> FetchHtmlAsync(string title);
    }
}