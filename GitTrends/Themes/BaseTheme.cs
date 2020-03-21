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

            Add(nameof(TrendsChartSettingsLabelTextColor), TrendsChartSettingsLabelTextColor);
            Add(nameof(TrendsChartSettingsBorderColor), TrendsChartSettingsBorderColor);
            Add(nameof(TrendsChartSettingsFontColor), TrendsChartSettingsFontColor);
            Add(nameof(TrendsChartSettingsSelectionIndicatorColor), TrendsChartSettingsSelectionIndicatorColor);

            Add(nameof(GitTrendsImageBackgroundColor), GitTrendsImageBackgroundColor);
        }

        public abstract Color NavigationBarBackgroundColor { get; }
        public abstract Color NavigationBarTextColor { get; }

        public abstract Color PageBackgroundColor { get; }

        public abstract Color PrimaryTextColor { get; }
        public abstract Color TextColor { get; }

        public abstract Color RefreshControlColor { get; }

        //Chart
        public abstract Color TotalViewsColor { get; }
        public abstract Color TotalUniqueViewsColor { get; }
        public abstract Color TotalClonesColor { get; }
        public abstract Color TotalUniqueClonesColor { get; }

        public abstract Color ChartAxisTextColor { get; }
        public abstract Color ChartAxisLineColor { get; }

        //Components
        public abstract Color SplashScreenStatusColor { get; }

        //Icons
        public abstract Color IconColor { get; }
        public abstract Color IconPrimaryColor { get; }

        //Buttons
        public abstract Color ButtonTextColor { get; }
        public abstract Color ButtonBackgroundColor { get; }

        //Card Stats Color
        public abstract Color CardStarsStatsTextColor { get; }
        public abstract Color CardStarsStatsIconColor { get; }
        public abstract Color CardForksStatsTextColor { get; }
        public abstract Color CardForksStatsIconColor { get; }
        public abstract Color CardIssuesStatsTextColor { get; }
        public abstract Color CardIssuesStatsIconColor { get; }
        public abstract Color CardViewsStatsTextColor { get; }
        public abstract Color CardViewsStatsIconColor { get; }
        public abstract Color CardClonesStatsTextColor { get; }
        public abstract Color CardClonesStatsIconColor { get; }
        public abstract Color CardUniqueViewsStatsTextColor { get; }
        public abstract Color CardUniqueViewsStatsIconColor { get; }
        public abstract Color CardUniqueClonesStatsTextColor { get; }
        public abstract Color CardUniqueClonesStatsIconColor { get; }
        public abstract Color CardTrendingStatsColor { get; }

        //Trends Settings Component
        public abstract Color TrendsChartSettingsLabelTextColor { get; }
        public abstract Color TrendsChartSettingsBorderColor { get; }
        public abstract Color TrendsChartSettingsFontColor { get; }
        public abstract Color TrendsChartSettingsSelectionIndicatorColor { get; }

        public abstract Color GitTrendsImageBackgroundColor { get; }
    }
}
