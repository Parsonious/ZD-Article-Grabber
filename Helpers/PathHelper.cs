using System.Runtime;
using System.Text;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Helpers
{
    public class PathHelper : IPathHelper
    {
        // Map of characters to their URL encoded values per RFC 3986 standard
        private static readonly Dictionary<char, string> UrlEncodingMap = new()
    {
        {' ', "%20"}, {'!', "%21"}, {'"', "%22"}, {'#', "%23"}, {'$', "%24"}, {'%', "%25"}, {'&', "%26"},
        {'\'', "%27"}, {'(', "%28"}, {')', "%29"}, {'*', "%2A"}, {'+', "%2B"}, {',', "%2C"}, {'/', "%2F"},
        {':', "%3A"}, {';', "%3B"}, {'<', "%3C"}, {'=', "%3D"}, {'>', "%3E"}, {'?', "%3F"}, {'@', "%40"},
        {'[', "%5B"}, {'\\', "%5C"}, {']', "%5D"}, {'^', "%5E"}, {'`', "%60"}, {'{', "%7B"},{'|', "%7C"},
        {'}', "%7D"}, {'~', "%7E"}
    };
        public string NormalizeTitle(string title)
        {
            return SanitizeInput(RemoveExtension(title));
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
            ResourceType.CSS => ".css",
            ResourceType.HTML => ".html",
            ResourceType.IMG => ".img",
            ResourceType.JS => ".js",
            ResourceType.PS1 => ".ps1",
            ResourceType.SQL => ".sql",
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
        public string EncodeUrl (string preEncodedUrl)
        {
            if ( string.IsNullOrEmpty(preEncodedUrl) )
            {
                return preEncodedUrl;
            }

            StringBuilder encodedString = new(preEncodedUrl.Length * 2); //pre allocate capacity assuming SOME characters will be encoded
            foreach ( char c in preEncodedUrl )
            {
                if ( UrlEncodingMap.TryGetValue(c, out string? value) )
                {
                    encodedString.Append(value);
                }
                else
                {
                    encodedString.Append(c);
                }
            }
            return encodedString.ToString();
        }

        //Get the difference between two paths this always ignores case and assumes the absolutePath is always the right path 
        //and will return absolutePath if the base path is not a part of the absolute path
        public string GetPathDifference(string basePath, string absolutePath)
        {
            //remove breaking drive letter if exists
            // Remove breaking drive letter if exists
            if ( Path.IsPathRooted(absolutePath) )
            {
                absolutePath = absolutePath[Path.GetPathRoot(absolutePath).Length..]; // Substring from the length of the root path to the end of the string
            }

            // Ensure both paths use forward slashes (this is for a URL after all)
            basePath = basePath.Replace('\\', '/').TrimEnd('/');
            absolutePath = absolutePath.Replace('\\', '/').TrimEnd('/');

            string[] baseSegments = basePath.Split('/');
            string[] absoluteSegments = absolutePath.Split('/'); //initialize this with an incorrect value

            if (absolutePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                absoluteSegments = absolutePath.Substring(8).Split('/');
            }
            else if (absolutePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                absoluteSegments = absolutePath.Substring(7).Split('/');
            }

            int index = 0;
            // While the index is less than the length of the base and absolute paths and the segments are equal, increment the index
            while ( index < baseSegments.Length && index < absoluteSegments.Length && string.Equals(baseSegments[index], absoluteSegments[index], StringComparison.OrdinalIgnoreCase) )
            {
                index++;
            }

            // Get the different segments
            var diffSegments = absoluteSegments.Skip(index);
            
            return string.Join("/", diffSegments);
        }

        //return the difference between two paths with the option to compare case
        //This will account for upward directory traversal and assumes the base path of the two paths is the same even when there are no similarities in the paths
        public string PathDiff(string path1, string path2, bool compareCase)
        {
            char separator = Path.DirectorySeparatorChar;
            int num = -1;
            int i;
            for ( i = 0; i < path1.Length && i < path2.Length && (path1[i] == path2[i] || (!compareCase && char.ToLowerInvariant(path1[i]) == char.ToLowerInvariant(path2[i]))); i++ )
            {
                if ( path1[i] == separator )
                {
                    num = i;
                }
            }
            if ( i == 0 )
            {
                return path2;
            }
            if ( i == path1.Length && i == path2.Length )
            {
                return string.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder();
            for ( ; i < path1.Length; i++ )
            {
                if ( path1[i] == '/' )
                {
                    stringBuilder.Append("../");
                }
            }
            if ( stringBuilder.Length == 0 && path2.Length - 1 == num )
            {
                return $".{separator}";
            }
            return stringBuilder.Append(path2.AsSpan(num + 1)).ToString();
        }
        public string CompleteLocalPath(string basePath, string extractedPath, ResourceType type)
        {
            //set type string to lowercase
            string typeString = type.ToString().ToLower();

            //remove any .. from the path
            while ( extractedPath.StartsWith("..") )
            {
                extractedPath = extractedPath[2..]; //WAN for .Substring(2);
            }

            //set OS specific path separators
            extractedPath = extractedPath.Replace('\\', '/').TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            //Check for preexisting type in path
            if ( extractedPath.StartsWith(typeString, StringComparison.OrdinalIgnoreCase) ) //Set up a StringComparison to remove an allocation for the ToLower call
            {
                return Path.Combine(basePath, extractedPath);
            }

            //finalize the path by combining the resource path with the extracted path
            return Path.Combine(basePath, typeString, extractedPath);
        }
    }
}
