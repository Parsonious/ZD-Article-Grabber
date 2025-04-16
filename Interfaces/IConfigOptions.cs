using System.ComponentModel.DataAnnotations;
using ZD_Article_Grabber.Config;

namespace ZD_Article_Grabber.Interfaces
{
    public interface IConfigOptions
    {
        PathsConfig Paths { get; }
        FilesConfig Files { get; }
        Dictionary<string, string> XPathQueries { get; }
    }

    public class ConfigOptions : IConfigOptions
    {
        public required PathsConfig Paths { get; set; }
        public required FilesConfig Files { get; set; }
        public required Dictionary<string, string> XPathQueries { get; set; }
    }
}
