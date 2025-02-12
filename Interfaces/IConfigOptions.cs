using System.ComponentModel.DataAnnotations;
using ZD_Article_Grabber.Config;

namespace ZD_Article_Grabber.Interfaces
{
    public interface IConfigOptions
    {
        PathsConfig Paths { get; }
        FilesConfig Files { get; }
        JwtConfig Jwt { get; }
        Dictionary<string, string> XPathQueries { get; }
    }
}
