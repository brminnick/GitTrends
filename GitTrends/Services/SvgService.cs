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

        public static IReadOnlyDictionary<string, string> GetColorStringMap(in string hex)
        {
            if (!hex.StartsWith("#"))
                throw new ArgumentException($"{nameof(hex)} must begin with `#`");

            return new Dictionary<string, string> { { "#000000", hex } };
        }

        internal static IReadOnlyDictionary<string, string> GetColorStringMap(in Xamarin.Forms.Color color) => GetColorStringMap(color.ToHex());
    }
}
