using HtmlAgilityPack;

namespace ZD_Article_Grabber.Classes
{
    public static class PathHelper
    {
        //this class is to be used to pull from a config file the base path and the default url for remote access it obviously needs to be expanded out quite a bit.
        private static string NormalizeTitle(string title)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach ( char c in invalidChars )
            {
                title = title.Replace(c.ToString(), "");
            }
            return title.Replace(" ", "%20");
        }
        public static string CombineAndValidatePath(string baseDirectory, string relativePath)
        {
            string combinedPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));

            // Ensure that the combined path starts with the base directory
            if ( !combinedPath.StartsWith(Path.GetFullPath(baseDirectory)) )
            {
                throw new UnauthorizedAccessException("Access to the path is denied.");
            }

            return combinedPath;
        }

    }
}
