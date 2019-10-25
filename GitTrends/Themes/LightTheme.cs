using Xamarin.Forms;

namespace GitTrends
{
    public class LightTheme : BaseTheme
    {
        const string _darkestBlueHex = "#2C7797";
        const string _darkBlueHex = "#185E7C";
        const string _mediumBlueHex = "#1FAECE";
        const string _lightBlueHex = "#96E2F5";
        const string _gitHubColorHex = "#333333";
        const string _lightNavyBlueHex = "#365271";
        const string _darkNavyBlueHex = "#1C2B39";

        public override Color NavigationBarBackgroundColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color NavigationBarTextColor { get; } = Color.White;

        public override Color PageBackgroundColor { get; } = Color.FromHex(_lightBlueHex);

        public override Color TextColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color RefreshControlColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color TotalViewsColor { get; } = Color.FromHex(_darkestBlueHex);
        public override Color TotalUniqueViewsColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color TotalClonesColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TotalUniqueClonesColor { get; } = Color.FromHex(_lightNavyBlueHex);

        public override Color ChartAxisTextColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color ChartAxisLineColor { get; } = Color.FromHex(_lightNavyBlueHex);

        public override Color ButtonBackgroundColor { get; } = Color.FromHex(_darkNavyBlueHex);

        public override Color TrendsChartSettingsLabelTextColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TrendsChartSettingsBorderColor { get; } = Color.FromHex(_darkBlueHex);
        public override Color TrendsChartSettingsFontColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromHex(_darkBlueHex);
    }
}
