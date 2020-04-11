using System.Collections.Generic;

namespace GitTrends
{
    public static class SvgService
    {
        public static string GetResourcePath(in string fileName) => $"{nameof(GitTrends)}.Resources.SVGs.{fileName}";
        public static string GetFullPath(in string fileName) => $"resource://{GetResourcePath(fileName)}";

        public static Dictionary<string, string> GetColorStringMap(in Xamarin.Forms.Color color) => GetColorStringMap(color.ToHex());
        public static Dictionary<string, string> GetColorStringMap(in string hex) => new Dictionary<string, string> { { "#000000", hex } };
    }
}
