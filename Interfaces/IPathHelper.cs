using ZD_Article_Grabber.Resources;

namespace ZD_Article_Grabber.Interfaces
{
    public interface IPathHelper
    {
        string NormalizeTitle(string title);
        string GetUrlTitle(string title);
        string CombineAndValidatePath(string baseDirectory, string relativePath);
        string GetExtension(ResourceType type);
        string EncodeUrl(string input);
        string GetPathDifference(string basePath, string path);
        string PathDiff(string path1, string path2, bool compareCase);
        string CompleteLocalPath(string basePath, string extractedPath, ResourceType type);
    }
}