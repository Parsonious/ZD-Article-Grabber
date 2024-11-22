using ZD_Article_Grabber.Resources;

namespace ZD_Article_Grabber.Interfaces
{
    public interface IPathHelper
    {
        string NormalizeTitle(string title);
        string GetUrlTitle(string title);
        string CombineAndValidatePath(string baseDirectory, string relativePath);
        string GetExtension(ResourceType type); 
    }
}