using Xamarin.Forms;

namespace GitTrends
{
    public class DarkTheme : BaseTheme
    {
        const string _darkestBlueHex = "#1D566E";
        const string _darkBlueHex = "#08232E";
        const string _mediumBlueHex = "#12687C";
        const string _lightBlueHex = "#74B4C3";

        const string _darkNavyBlueHex = "#0E1820";
        const string _lightNavyBlueHex = "#273D56";

        const string _gitHubColorHex = "#333333";

        const string _offWhite = "#F0EFEF";

        public override Color NavigationBarBackgroundColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color NavigationBarTextColor { get; } = Color.FromHex(_offWhite);

        public override Color PageBackgroundColor { get; } = Color.FromHex(_lightBlueHex);

        public override Color TextColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color RefreshControlColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color TotalViewsColor { get; } = Color.FromHex(_darkestBlueHex);
        public override Color TotalUniqueViewsColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color TotalClonesColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TotalUniqueClonesColor { get; } = Color.FromHex(_lightNavyBlueHex);

        public override Color ChartAxisTextColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color ChartAxisLineColor { get; } = Color.FromHex(_lightNavyBlueHex);

        public override Color ButtonTextColor { get; } = Color.FromHex(_offWhite);
        public override Color ButtonBackgroundColor { get; } = Color.FromHex(_darkNavyBlueHex);

        public override Color TrendsChartSettingsLabelTextColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TrendsChartSettingsBorderColor { get; } = Color.FromHex(_darkBlueHex);
        public override Color TrendsChartSettingsFontColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color GitTrendsImageBackgroundColor { get; } = Color.FromHex(_offWhite);
    }
}
