using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Interfaces
{
    public interface IConfigOptions
    {
        PathsOptions Paths { get; }
        FilesOptions Files { get; }
        Dictionary<string, string> XPathQueries { get; }
    }

}
