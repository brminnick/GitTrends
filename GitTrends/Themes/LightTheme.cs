using Xamarin.Forms;

namespace GitTrends
{
    public class LightTheme : BaseTheme
    {
        const string _primaryTextHex = "#584053";
        const string _primaryTealHex = LightTealColorHex;
        const string _primaryDarkTealHex = "#1C473D";
        const string _accentYellowHex = "#FCBC66";
        const string _accentOrangeHex = CoralColorHex;
        const string _toolbarTextHex = "#FFFFFF";
        const string _textHex = "#121212";
        const string _iconColor = "#121212";
        const string _buttonTextColor = "#FFFFFF";
        const string _accentLightBlueHex = "#66A7FC";
        const string _accentPurpleHex = "#8F3795";
        const string _backgroundSurfaceHex = "#FFFFFF";
        const string _cardSurfaceHex = "#FFFFFF";
        const string _toolbarSurfaceHex = LightTealColorHex;
        const string _circleImageBackgroundHex = "#FFFFFF";
        const string _githubButtonSurfaceHex = "#231F20";

        const string _darkNavyBlueHex = "#1C2B39";
        const string _lightNavyBlueHex = "#365271";

        const string _gitHubColorHex = "#333333";

        public override Color NavigationBarBackgroundColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color NavigationBarTextColor { get; } = Color.White;

        public override Color PageBackgroundColor { get; } = Color.FromHex(_backgroundSurfaceHex);
        public override Color PageBackgroundColor_75Opactity { get; } = Color.FromHex(_backgroundSurfaceHex).MultiplyAlpha(0.75);

        public override Color TextColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color RefreshControlColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color TotalViewsColor { get; } = Color.FromHex(_darkestBlueHex);
        public override Color TotalUniqueViewsColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color TotalClonesColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TotalUniqueClonesColor { get; } = Color.FromHex(_lightNavyBlueHex);

        public override Color ChartAxisTextColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color ChartAxisLineColor { get; } = Color.FromHex(_lightNavyBlueHex);

        public override Color ButtonTextColor { get; } = Color.White;
        public override Color ButtonBackgroundColor { get; } = Color.FromHex(_darkNavyBlueHex);

        //Icons
        public override Color IconColor { get; } = Color.FromHex(_iconColor);
        public override Color IconPrimaryColor { get; } = Color.FromHex(_primaryTealHex);

        //Buttons
        public override Color ButtonTextColor { get; } = Color.FromHex(_buttonTextColor);
        public override Color ButtonBackgroundColor { get; } = Color.FromHex(_primaryTealHex);

        //Indicators
        public override Color ActivityIndicatorColor { get; } = Color.FromHex(_primaryDarkTealHex);
        public override Color PullToRefreshColor { get; } = Color.FromHex(_toolbarSurfaceHex);

        //Card
        public override Color CardSurfaceColor { get; } = Color.FromHex(_cardSurfaceHex);
        public override Color CardBorderColor { get; } = Color.FromHex(_black12PercentBlend);

        public override Color SeparatorColor { get; } = Color.FromHex(_black12PercentBlend);

        //Card Stats Color
        public override Color CardStarsStatsTextColor { get; } = Color.FromHex(_textHex);
        public override Color CardStarsStatsIconColor { get; } = Color.FromHex(_accentYellowHex);
        public override Color CardForksStatsTextColor { get; } = Color.FromHex(_textHex);
        public override Color CardForksStatsIconColor { get; } = Color.FromHex(_primaryTealHex);
        public override Color CardIssuesStatsTextColor { get; } = Color.FromHex(_textHex);
        public override Color CardIssuesStatsIconColor { get; } = Color.FromHex(_accentOrangeHex);
        public override Color CardViewsStatsTextColor { get; } = Color.FromHex(_textHex);
        public override Color CardViewsStatsIconColor { get; } = Color.FromHex(_saturatedLightBlueHex);
        public override Color CardClonesStatsTextColor { get; } = Color.FromHex(_textHex);
        public override Color CardClonesStatsIconColor { get; } = Color.FromHex(_saturatedYellowHex);
        public override Color CardUniqueViewsStatsTextColor { get; } = Color.FromHex(_textHex);
        public override Color CardUniqueViewsStatsIconColor { get; } = Color.FromHex(_saturatedIndigoHex);
        public override Color CardUniqueClonesStatsTextColor { get; } = Color.FromHex(_textHex);
        public override Color CardUniqueClonesStatsIconColor { get; } = Color.FromHex(_saturatedPinkHex);
        public override Color CardTrendingStatsColor { get; } = Color.FromHex(_primaryTealHex);

        //Settings Components
        public override Color SettingsLabelTextColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color BorderButtonBorderColor { get; } = Color.FromHex(_black12PercentBlend);
        public override Color BorderButtonFontColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromHex(_primaryTealHex);

        public override Color GitTrendsImageBackgroundColor { get; } = Color.White;

        public override Color GitHubButtonSurfaceColor { get; } = Color.FromHex(_githubButtonSurfaceHex);

        public override Color GitHubHandleColor { get; } = Color.FromHex(_textHex);

        public override Color PrimaryColor { get; } = Color.FromHex(_primaryTealHex);

        public override string GitTrendsImageSource { get; } = "GitTrendsGreen";
        public override string DefaultProfileImageSource { get; } = "DefaultProfileImageGreen";
        public override string DefaultReferringSiteImageSource { get; } = "DefaultReferringSiteImage";
    }
}
