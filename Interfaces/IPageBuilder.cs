using ZD_Article_Grabber.Resources.Pages;

namespace ZD_Article_Grabber.Interfaces
{
    public interface IPageBuilder
    {
        Task<Page> BuildPageAsync(string title);
    }
}