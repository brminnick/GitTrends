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

            Add(nameof(PrimaryTextColor), PrimaryTextColor);
            Add(nameof(TextColor), TextColor);

            Add(nameof(ActivityIndicatorColor), ActivityIndicatorColor);
            Add(nameof(PullToRefreshColor), PullToRefreshColor);

            Add(nameof(TotalViewsColor), TotalViewsColor);
            Add(nameof(TotalUniqueViewsColor), TotalUniqueViewsColor);
            Add(nameof(TotalClonesColor), TotalClonesColor);
            Add(nameof(TotalUniqueClonesColor), TotalUniqueClonesColor);

            Add(nameof(ChartAxisTextColor), ChartAxisTextColor);
            Add(nameof(ChartAxisLineColor), ChartAxisLineColor);

            Add(nameof(SplashScreenStatusColor), SplashScreenStatusColor);

            Add(nameof(IconColor), IconColor);
            Add(nameof(IconPrimaryColor), IconPrimaryColor);

            Add(nameof(ButtonTextColor), ButtonTextColor);
            Add(nameof(ButtonBackgroundColor), ButtonBackgroundColor);

            Add(nameof(CardSurfaceColor), CardSurfaceColor);
            Add(nameof(CardBorderColor), CardBorderColor);

            Add(nameof(SeparatorColor), SeparatorColor);

            Add(nameof(CardStarsStatsTextColor), CardStarsStatsTextColor);
            Add(nameof(CardStarsStatsIconColor), CardStarsStatsIconColor);
            Add(nameof(CardForksStatsTextColor), CardForksStatsTextColor);
            Add(nameof(CardForksStatsIconColor), CardForksStatsIconColor);
            Add(nameof(CardIssuesStatsTextColor), CardIssuesStatsTextColor);
            Add(nameof(CardIssuesStatsIconColor), CardIssuesStatsIconColor);
            Add(nameof(CardViewsStatsTextColor), CardViewsStatsTextColor);
            Add(nameof(CardViewsStatsIconColor), CardViewsStatsIconColor);
            Add(nameof(CardClonesStatsTextColor), CardClonesStatsTextColor);
            Add(nameof(CardClonesStatsIconColor), CardClonesStatsIconColor);
            Add(nameof(CardUniqueViewsStatsTextColor), CardUniqueViewsStatsTextColor);
            Add(nameof(CardUniqueViewsStatsIconColor), CardUniqueViewsStatsIconColor);
            Add(nameof(CardUniqueClonesStatsTextColor), CardUniqueClonesStatsTextColor);
            Add(nameof(CardUniqueClonesStatsIconColor), CardUniqueClonesStatsIconColor);
            Add(nameof(CardTrendingStatsColor), CardTrendingStatsColor);

            Add(nameof(SettingsLabelTextColor), SettingsLabelTextColor);
            Add(nameof(SettingsButtonBorderColor), SettingsButtonBorderColor);
            Add(nameof(SettingsButtonFontColor), SettingsButtonFontColor);
            Add(nameof(TrendsChartSettingsSelectionIndicatorColor), TrendsChartSettingsSelectionIndicatorColor);

            Add(nameof(GitTrendsImageBackgroundColor), GitTrendsImageBackgroundColor);

            Add(nameof(GitHubButtonSurfaceColor), GitHubButtonSurfaceColor);
        }

        public abstract Color NavigationBarBackgroundColor { get; }
        public abstract Color NavigationBarTextColor { get; }

        public abstract Color PageBackgroundColor { get; }

        public abstract Color PrimaryTextColor { get; }
        public abstract Color TextColor { get; }

        public abstract Color ActivityIndicatorColor { get; }
        public abstract Color PullToRefreshColor { get; }

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

        //Card
        public abstract Color CardSurfaceColor { get; }
        public abstract Color CardBorderColor { get; }

        public abstract Color SeparatorColor { get; }

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

        //Settings Components
        public abstract Color SettingsLabelTextColor { get; }
        public abstract Color SettingsButtonBorderColor { get; }
        public abstract Color SettingsButtonFontColor { get; }
        public abstract Color TrendsChartSettingsSelectionIndicatorColor { get; }

        public abstract Color GitTrendsImageBackgroundColor { get; }

        public abstract Color GitHubButtonSurfaceColor { get; }
    }
}
