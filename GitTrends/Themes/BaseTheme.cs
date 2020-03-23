using Xamarin.Forms;

namespace GitTrends
{
    public enum Theme { Light, Dark };

    public abstract class BaseTheme : ResourceDictionary
    {
        protected BaseTheme()
        {
            Add(nameof(NavigationBarBackgroundColor), NavigationBarBackgroundColor);
            Add(nameof(NavigationBarTextColor), NavigationBarTextColor);

            Add(nameof(PageBackgroundColor), PageBackgroundColor);

            Add(nameof(TextColor), TextColor);

            Add(nameof(RefreshControlColor), RefreshControlColor);

            Add(nameof(TotalViewsColor), TotalViewsColor);
            Add(nameof(TotalUniqueViewsColor), TotalUniqueViewsColor);
            Add(nameof(TotalClonesColor), TotalClonesColor);
            Add(nameof(TotalUniqueClonesColor), TotalUniqueClonesColor);

            Add(nameof(ChartAxisTextColor), ChartAxisTextColor);
            Add(nameof(ChartAxisLineColor), ChartAxisLineColor);

            Add(nameof(ButtonTextColor), ButtonTextColor);
            Add(nameof(ButtonBackgroundColor), ButtonBackgroundColor);

            Add(nameof(SettingsLabelTextColor), SettingsLabelTextColor);
            Add(nameof(SettingsButtonBorderColor), SettingsButtonBorderColor);
            Add(nameof(SettingsButtonFontColor), SettingsButtonFontColor);
            Add(nameof(TrendsChartSettingsSelectionIndicatorColor), TrendsChartSettingsSelectionIndicatorColor);

            Add(nameof(GitTrendsImageBackgroundColor), GitTrendsImageBackgroundColor);
        }

        public abstract Color NavigationBarBackgroundColor { get; }
        public abstract Color NavigationBarTextColor { get; }

        public abstract Color PageBackgroundColor { get; }

        public abstract Color TextColor { get; }

        public abstract Color RefreshControlColor { get; }

        public abstract Color TotalViewsColor { get; }
        public abstract Color TotalUniqueViewsColor { get; }
        public abstract Color TotalClonesColor { get; }
        public abstract Color TotalUniqueClonesColor { get; }

        public abstract Color ChartAxisTextColor { get; }
        public abstract Color ChartAxisLineColor { get; }

        public abstract Color ButtonTextColor { get; }
        public abstract Color ButtonBackgroundColor { get; }

        public abstract Color SettingsLabelTextColor { get; }
        public abstract Color SettingsButtonBorderColor { get; }
        public abstract Color SettingsButtonFontColor { get; }
        public abstract Color TrendsChartSettingsSelectionIndicatorColor { get; }

        public abstract Color GitTrendsImageBackgroundColor { get; }
    }
}
