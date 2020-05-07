using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseTheme : ResourceDictionary
    {
        public const string LightTealColorHex = "338F82";
        public const string CoralColorHex = "F97B4F";

        protected BaseTheme()
        {
            Add(nameof(NavigationBarBackgroundColor), NavigationBarBackgroundColor);
            Add(nameof(NavigationBarTextColor), NavigationBarTextColor);

            Add(nameof(PageBackgroundColor), PageBackgroundColor);
            Add(nameof(PageBackgroundColor_75Opactity), PageBackgroundColor_75Opactity);

            Add(nameof(TextColor), TextColor);

            Add(nameof(ActivityIndicatorColor), ActivityIndicatorColor);
            Add(nameof(PullToRefreshColor), PullToRefreshColor);

            Add(nameof(TotalViewsColor), TotalViewsColor);
            Add(nameof(TotalUniqueViewsColor), TotalUniqueViewsColor);
            Add(nameof(TotalClonesColor), TotalClonesColor);
            Add(nameof(TotalUniqueClonesColor), TotalUniqueClonesColor);

            Add(nameof(ChartAxisTextColor), ChartAxisTextColor);
            Add(nameof(ChartAxisLineColor), ChartAxisLineColor);

            Add(nameof(ButtonTextColor), ButtonTextColor);
            Add(nameof(ButtonBackgroundColor), ButtonBackgroundColor);

            Add(nameof(SettingsLabelTextColor), SettingsLabelTextColor);
            Add(nameof(BorderButtonBorderColor), BorderButtonBorderColor);
            Add(nameof(BorderButtonFontColor), BorderButtonFontColor);
            Add(nameof(TrendsChartSettingsSelectionIndicatorColor), TrendsChartSettingsSelectionIndicatorColor);

            Add(nameof(GitTrendsImageBackgroundColor), GitTrendsImageBackgroundColor);

            Add(nameof(GitHubButtonSurfaceColor), GitHubButtonSurfaceColor);
            Add(nameof(GitHubHandleColor), GitHubHandleColor);

            Add(nameof(PrimaryColor), PrimaryColor);

            Add(nameof(GitTrendsImageSource), GitTrendsImageSource);
            Add(nameof(DefaultProfileImageSource), DefaultProfileImageSource);
            Add(nameof(DefaultReferringSiteImageSource), DefaultReferringSiteImageSource);
        }

        public static string GetGitTrendsImageSource() => (string)(Application.Current?.Resources?[nameof(GitTrendsImageSource)] ?? "GitTrends");
        public static string GetDefaultProfileImageSource() => (string)(Application.Current?.Resources?[nameof(DefaultProfileImageSource)] ?? "DefaultProfileImage");
        public static string GetDefaultReferringSiteImageSource() => (string)(Application.Current?.Resources?[nameof(DefaultReferringSiteImageSource)] ?? "DefaultReferringSiteImage");

        public abstract Color NavigationBarBackgroundColor { get; }
        public abstract Color NavigationBarTextColor { get; }

        public abstract Color PageBackgroundColor { get; }
        public abstract Color PageBackgroundColor_75Opactity { get; }

        public abstract Color TextColor { get; }

        public abstract Color ActivityIndicatorColor { get; }
        public abstract Color PullToRefreshColor { get; }

        public abstract Color TotalViewsColor { get; }
        public abstract Color TotalUniqueViewsColor { get; }
        public abstract Color TotalClonesColor { get; }
        public abstract Color TotalUniqueClonesColor { get; }

        public abstract Color ChartAxisTextColor { get; }
        public abstract Color ChartAxisLineColor { get; }

        public abstract Color ButtonTextColor { get; }
        public abstract Color ButtonBackgroundColor { get; }

        public abstract Color SettingsLabelTextColor { get; }
        public abstract Color BorderButtonBorderColor { get; }
        public abstract Color BorderButtonFontColor { get; }
        public abstract Color TrendsChartSettingsSelectionIndicatorColor { get; }

        public abstract Color GitTrendsImageBackgroundColor { get; }

        public abstract Color GitHubButtonSurfaceColor { get; }
        public abstract Color GitHubHandleColor { get; }

        public abstract Color PrimaryColor { get; }

        public abstract string GitTrendsImageSource { get; }
        public abstract string DefaultProfileImageSource { get; }
        public abstract string DefaultReferringSiteImageSource { get; }
    }
}
