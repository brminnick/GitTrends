using System.Collections.Generic;

namespace GitTrends
{
    public static class SVGService
    {
        public static string GetSVGResourcePath(string fileName) => $"resource://{nameof(GitTrends)}.Resources.{fileName}";

        public static Dictionary<string, string> GetColorStringMap(Xamarin.Forms.Color color) => GetColorStringMap(color.ToHex());
        public static Dictionary<string, string> GetColorStringMap(string hex) => new Dictionary<string, string> { { "#000000", hex } };
    }
}
