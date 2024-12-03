using System.Text;
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
            var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()));
            var sanitizedInput = new StringBuilder(input.Length);

            foreach ( char c in input )
            {
                if ( !invalidChars.Contains(c) && c != '%' && c != '/' && c != '\\' )
                {
                    sanitizedInput.Append(c);
                }
            }

            return sanitizedInput.ToString();
        }
        internal static string RemoveExtension(string input)
        {
            if ( Path.HasExtension(input) )
            {
                return Path.GetFileNameWithoutExtension(input);
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
            if (!combinedPath.StartsWith(Path.GetFullPath(baseDirectory), StringComparison.OrdinalIgnoreCase) )
            {
                throw new UnauthorizedAccessException("Access to the path is denied.");
            }

            return combinedPath;
        }

    }
}
