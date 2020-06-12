using Sharpnado.MaterialFrame;
using Xamarin.Forms;

namespace GitTrends
{
    public class DarkTheme : BaseTheme
    {
        const string _offWhite = "#F0EFEF";

        const string _primaryTextHex = "#CCBAC4";
        const string _primaryTealHex = "#98CDC6";
        const string _accentYellowHex = "#FDD59B";
        const string _accentOrangeHex = "#FCBAA2";
        const string _toolbarTextHex = "#FFFFFF";
        const string _textHex = "#FFFFFF";
        const string _iconColor = "#FFFFFF";
        const string _buttonTextColor = "#FFFFFF";
        const string _accentLightBlueHex = "#F0EFEF";
        const string _accentPurpleHex = "#C795CA";
        const string _accentIndigoHex = "#5D6AB1";
        const string _backgroundSurfaceHex = "#121212";
        const string _cardSurfaceHex = "#1D2221";
        const string _toolbarSurfaceHex = "#1E2423";
        const string _circleImageBackgroundHex = "#FFFFFF";
        const string _githubButtonSurfaceHex = "#231F20";

        //Saturated colors from light theme
        const string _lightAccentYellowHex = "#FCBC66";
        const string _lightAccentOrangeHex = CoralColorHex;
        const string _lightAccentLightBlueHex = "#66A7FC";
        const string _lightAccentPurpleHex = "#8F3795";
        const string _lightPrimaryTealHex = LightTealColorHex;

        //Graph Palette
        const string _saturatedLightBlueHex = "#2BC3BE";
        const string _saturatedIndigoHex = "#5D6AB1";
        const string _saturatedYellowHex = "#FFC534";
        const string _saturatedPinkHex = "#F2726F";

        //Blended Colors
        const string _white12PercentBlend = "#2F2F2F";

        //Surface Elvations
        const string _surfaceElevation0dpHex = "#121212";
        const string _surfaceElevation1dpHex = "#181B1B";
        const string _surfaceElevation2dpHex = "#1B1F1F";
        const string _surfaceElevation3dpHex = "#1C2120";
        const string _surfaceElevation4dpHex = "#1D2221";
        const string _surfaceElevation6dpHex = "#1F2625";
        const string _surfaceElevation8dpHex = "#212827";
        const string _surfaceElevation12dpHex = "#232B2A";
        const string _surfaceElevation16dpHex = "#242D2C";
        const string _surfaceElevation24dpHex = "#262F2E";

        //Surfaces
        public override Color NavigationBarBackgroundColor { get; } = Color.FromHex(_toolbarSurfaceHex);
        public override Color NavigationBarTextColor { get; } = Color.FromHex(_toolbarTextHex);

        public override Color PageBackgroundColor { get; } = Color.FromHex(_backgroundSurfaceHex);
        public override Color PageBackgroundColor_85Opactity { get; } = Color.FromHex(_backgroundSurfaceHex).MultiplyAlpha(0.85);

        //Text
        public override Color PrimaryTextColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color TextColor { get; } = Color.FromHex(_textHex);

        //Chart
        public override Color TotalViewsColor { get; } = Color.FromHex(_saturatedLightBlueHex);
        public override Color TotalUniqueViewsColor { get; } = Color.FromHex(_saturatedIndigoHex);
        public override Color TotalClonesColor { get; } = Color.FromHex(_saturatedYellowHex);
        public override Color TotalUniqueClonesColor { get; } = Color.FromHex(_saturatedPinkHex);

        public override Color ChartAxisTextColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color ChartAxisLineColor { get; } = Color.FromHex(_primaryTextHex);

        //Components
        //Splash
        public override Color SplashScreenStatusColor { get; } = Color.FromHex(_offWhite);

        //Icons
        public override Color IconColor { get; } = Color.FromHex(_primaryTealHex);
        public override Color IconPrimaryColor { get; } = Color.FromHex(_lightPrimaryTealHex);

        //Buttons
        public override Color ButtonTextColor { get; } = Color.FromHex(_lightPrimaryTealHex);
        public override Color ButtonBackgroundColor { get; } = Color.FromHex(_buttonTextColor);

        //Indicators
        public override Color ActivityIndicatorColor { get; } = Color.FromHex(_lightPrimaryTealHex);
        public override Color PullToRefreshColor { get; } = Device.RuntimePlatform is Device.iOS ? Color.FromHex(_lightPrimaryTealHex) : Color.FromHex(_toolbarSurfaceHex);

        //Card
        public override Color CardSurfaceColor { get; } = Color.FromHex(_cardSurfaceHex);
        public override Color CardBorderColor { get; } = Color.FromHex(_cardSurfaceHex);

        public override Color SeparatorColor { get; } = Color.FromHex(_white12PercentBlend);

        //Card Stats Color
        public override Color CardStarsStatsTextColor { get; } = Color.FromHex(_lightAccentYellowHex);
        public override Color CardStarsStatsIconColor { get; } = Color.FromHex(_lightAccentYellowHex);
        public override Color CardForksStatsTextColor { get; } = Color.FromHex(_lightPrimaryTealHex);
        public override Color CardForksStatsIconColor { get; } = Color.FromHex(_lightPrimaryTealHex);
        public override Color CardIssuesStatsTextColor { get; } = Color.FromHex(_lightAccentOrangeHex);
        public override Color CardIssuesStatsIconColor { get; } = Color.FromHex(_lightAccentOrangeHex);
        public override Color CardViewsStatsTextColor { get; } = Color.FromHex(_saturatedLightBlueHex);
        public override Color CardViewsStatsIconColor { get; } = Color.FromHex(_saturatedLightBlueHex);
        public override Color CardClonesStatsTextColor { get; } = Color.FromHex(_saturatedYellowHex);
        public override Color CardClonesStatsIconColor { get; } = Color.FromHex(_saturatedYellowHex);
        public override Color CardUniqueViewsStatsTextColor { get; } = Color.FromHex(_saturatedIndigoHex);
        public override Color CardUniqueViewsStatsIconColor { get; } = Color.FromHex(_saturatedIndigoHex);
        public override Color CardUniqueClonesStatsTextColor { get; } = Color.FromHex(_saturatedPinkHex);
        public override Color CardUniqueClonesStatsIconColor { get; } = Color.FromHex(_saturatedPinkHex);
        public override Color CardTrendingStatsColor { get; } = Color.FromHex(_primaryTealHex);

        //Settings Components
        public override Color SettingsLabelTextColor { get; } = Color.FromHex(_textHex);
        public override Color BorderButtonBorderColor { get; } = Color.FromHex(_white12PercentBlend);
        public override Color BorderButtonFontColor { get; } = Color.FromHex(_textHex);
        public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromHex(_lightPrimaryTealHex);

        public override Color GitTrendsImageBackgroundColor { get; } = Color.FromHex("4CADA2");

        public override Color GitHubButtonSurfaceColor { get; } = Color.FromHex(_githubButtonSurfaceHex);
        public override Color GitHubHandleColor { get; } = Color.FromHex(_primaryTextHex);

        public override Color PrimaryColor { get; } = Color.FromHex(_primaryTealHex);

        public override Color CloseButtonTextColor { get; } = Color.FromHex(_toolbarTextHex);
        public override Color CloseButtonBackgroundColor { get; } = Color.FromHex(_cardSurfaceHex);

        public override string GitTrendsImageSource { get; } = "GitTrendsWhite";
        public override string DefaultProfileImageSource { get; } = "DefaultProfileImageWhite";
        public override string DefaultReferringSiteImageSource { get; } = "DefaultReferringSiteImage";

        public override MaterialFrame.Theme MaterialFrameTheme { get; } = MaterialFrame.Theme.Dark;
    }
}
