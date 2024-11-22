using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Resources;

namespace ZD_Article_Grabber.Helpers
{


    public class PathHelper : IPathHelper
    {
        public string NormalizeTitle(string title)
        {
            return SanitizeInput(RemoveExtension(title));
            //return title.Replace(" ", "%20");
        }
        internal static string SanitizeInput(string input)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach ( char c in invalidChars )
            {
                input = input.Replace(c.ToString(), "");
            }
            // Remove invalid characters or perform other sanitization as needed
            return input.Replace("%", "").Replace("/", "").Replace("\\", "");
        }
        internal static string RemoveExtension(string input)
        {
            if ( Path.HasExtension(input) )
            {
                input = input.Replace(Path.GetExtension(input), "");
                return input;
            }
            return input;
        }
        public string GetUrlTitle(string title)
        {
            if ( Path.HasExtension(title) )
            {
                title = RemoveExtension(title);
            }
            return title.Replace(" ", "%20");
        }
        public string GetExtension(ResourceType type) => type switch
        {
            ResourceType.Css => ".css",
            ResourceType.Html => ".html",
            ResourceType.Img => ".img",
            ResourceType.Js => ".js",
            ResourceType.Ps1 => ".ps1",
            ResourceType.Sql => ".sql",
            _ => throw new InvalidOperationException($"Unsupported node type: {type}")
        };

        public string CombineAndValidatePath(string baseDirectory, string relativePath)
        {
            string combinedPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));

            // Ensure that the combined path starts with the base directory
            if (!combinedPath.StartsWith(Path.GetFullPath(baseDirectory)))
            {
                throw new UnauthorizedAccessException("Access to the path is denied.");
            }

            return combinedPath;
        }

    }
}
