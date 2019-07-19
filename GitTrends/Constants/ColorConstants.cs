using Xamarin.Forms;

namespace GitTrends
{
    public static class ColorConstants
    {
        public const string DarkestBlueHex = "#2C7797";
        public const string DarkBlueHex = "#185E7C";
        public const string MediumBlueHex = "#1FAECE";
        public const string LightBlueHex = "#96E2F5";

        public const string GitHubColorHex = "#333333";

        public const string LightNavyBlueHex = "#365271";
        public const string DarkNavyBlueHex = "#1C2B39";

        public static Color DarkestBlue { get; } = Color.FromHex(DarkestBlueHex);
        public static Color DarkBlue { get; } = Color.FromHex(DarkBlueHex);
        public static Color MediumBlue { get; } = Color.FromHex(MediumBlueHex);
        public static Color LightBlue { get; } = Color.FromHex(LightBlueHex);

        public static Color LightNavyBlue { get; } = Color.FromHex(LightNavyBlueHex);
        public static Color DarkNavyBlue { get; } = Color.FromHex(DarkNavyBlueHex);

        public static Color GitHubColor { get; } = Color.FromHex(GitHubColorHex);

        public static Color ActivityIndicatorColor { get; } = Device.RuntimePlatform is Device.iOS ? Color.White : DarkBlue;
    }
}
