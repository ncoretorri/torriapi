using System.Collections.ObjectModel;

namespace Torri.Helpers
{
    public static class FileHelper
    {
        private static readonly ReadOnlyCollection<string> MovieExtensions = [".mkv", ".avi", ".mp4"];

        public static bool IsVideoFile(string path)
        {
            return MovieExtensions.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase))
                && !path.Contains("sample", StringComparison.OrdinalIgnoreCase);
        }
    }
}
