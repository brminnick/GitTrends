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

            Add(nameof(ButtonBackgroundColor), ButtonBackgroundColor);

            Add(nameof(TrendsChartSettingsLabelTextColor), TrendsChartSettingsLabelTextColor);
            Add(nameof(TrendsChartSettingsBorderColor), TrendsChartSettingsBorderColor);
            Add(nameof(TrendsChartSettingsFontColor), TrendsChartSettingsFontColor);
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

        public abstract Color ButtonBackgroundColor { get; }

        public abstract Color TrendsChartSettingsLabelTextColor { get; }
        public abstract Color TrendsChartSettingsBorderColor { get; }
        public abstract Color TrendsChartSettingsFontColor { get; }
        public abstract Color TrendsChartSettingsSelectionIndicatorColor { get; }

        public abstract Color GitTrendsImageBackgroundColor { get; }
    }
}
