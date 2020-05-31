using System;
using System.Collections.Generic;

namespace GitTrends
{
    public static class SvgService
    {
        public static string GetResourcePath(in string fileName)
        {
            if (!fileName.EndsWith(".svg"))
                throw new ArgumentException($"{nameof(fileName)} must end with `.svg`");

            return $"{nameof(GitTrends)}.Resources.SVGs.{fileName}";
        }

        public static string GetFullPath(in string fileName) => $"resource://{GetResourcePath(fileName)}";

        internal static Dictionary<string, string> GetColorStringMap(in Xamarin.Forms.Color color) => GetColorStringMap(color.ToHex());

        public static Dictionary<string, string> GetColorStringMap(in string hex)
        {
            if (!hex.StartsWith("#"))
                throw new ArgumentException($"{nameof(hex)} must begin with `#`");

            if (hex.Length != 9)
                throw new ArgumentException($"{nameof(hex)} must contain 9 characters, e.g. `#FF00FF00`");

            return new Dictionary<string, string> { { "#000000", hex } };
        }
    }
}
