using HtmlAgilityPack;

namespace ZD_Article_Grabber.Helpers
{
    public static class PathHelper
    {
        //need to remove extensions from the title if they exist. Additionally need to make the cachekey MUCH more secure than it currently is.
        //need to make sure that the local check is first and is correct....fml.
        public static string NormalizeTitle(string title)
        {
            return SanitizeInput(RemoveExtension(title));
            //return title.Replace(" ", "%20");
        }
        internal static string SanitizeInput(string input)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                input = input.Replace(c.ToString(), "");
            }
            // Remove invalid characters or perform other sanitization as needed
            return input.Replace(" ", "").Replace("%", "").Replace("/", "").Replace("\\", "");
        }
        internal static string RemoveExtension(string input)
        {
            if (Path.HasExtension(input))
            {
                input = input.Replace(Path.GetExtension(input), "");
                return input;
            }
            return input;
        }
        public static string GetUrlTitle(string title)
        {
            title = RemoveExtension(title);
            return title.Replace(" ", "%20");
        }
        public static string CombineAndValidatePath(string baseDirectory, string relativePath)
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
