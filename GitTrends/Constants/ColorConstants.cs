using Xamarin.Forms;

namespace GitTrends
{
    public static class ColorConstants
    {
        const string _darkestBlueHex = "#2C7797";
        const string _darkBlueHex = "#185E7C";
        const string _mediumBlueHex = "#1FAECE";
        const string _lightBlueHex = "#96E2F5";

        const string _gitHubColorHex = "#333333";

        const string _lightNavyBlueHex = "#365271";
        const string _darkNavyBlueHex = "#1C2B39";

        public static Color DarkestBlue { get; } = Color.FromHex(_darkestBlueHex);
        public static Color DarkBlue { get; } = Color.FromHex(_darkBlueHex);
        public static Color MediumBlue { get; } = Color.FromHex(_mediumBlueHex);
        public static Color LightBlue { get; } = Color.FromHex(_lightBlueHex);

        public static Color LightNavyBlue { get; } = Color.FromHex(_lightNavyBlueHex);
        public static Color DarkNavyBlue { get; } = Color.FromHex(_darkNavyBlueHex);

        public static Color GitHubColor { get; } = Color.FromHex(_gitHubColorHex);

        public static Color PullToRefreshActivityIndicatorColor { get; } = DarkBlue;
    }
}
