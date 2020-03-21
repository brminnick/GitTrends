using Xamarin.Forms;

namespace GitTrends
{
    public class LightTheme : BaseTheme
    {
        const string _primaryTextHex = "#584053";
        const string _primaryTealHex = "#338F82";
        const string _primaryDarkTealHex = "#1C473D";
        const string _accentYellowHex = "#FCBC66";
        const string _accentOrangeHex = "#F97B4F";
        const string _toolbarTextHex = "#FFFFFF";
        const string _textHex = "#121212";
        const string _buttonTextColor = "#FFFFFF";
        const string _accentLightBlueHex = "#66A7FC";
        const string _accentPurpleHex = "#8F3795";
        const string _accentIndigoHex = "#4251A4";
        const string _backgroundSurfaceHex = "#FFFFFF";
        const string _cardSurfaceHex = "#FFFFFF";
        const string _toolbarSurfaceHex = "#338F82";
        const string _circleImageBackgroundHex = "#FFFFFF";
        const string _githubButtonSurfaceHex = "#231F20";

        //Graph Palette
        const string _saturatedLightBlueHex = "#2BC3BE";
        const string _saturatedGreenHex = "#63B58F";
        const string _saturatedYellowHex = "#FFC534";
        const string _saturatedPinkHex = "#F2726F";

        //Blended Colors
        const string _black12PercentBlend = "#E0E0E0";

        //Surfaces
        public override Color NavigationBarBackgroundColor { get; } = Color.FromHex(_toolbarSurfaceHex);
        public override Color NavigationBarTextColor { get; } = Color.FromHex(_toolbarTextHex);

        public override Color PageBackgroundColor { get; } = Color.FromHex(_backgroundSurfaceHex);

        //Text
        public override Color PrimaryTextColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color TextColor { get; } = Color.FromHex(_textHex);


        //Chart
        public override Color TotalViewsColor { get; } = Color.FromHex(_saturatedLightBlueHex);
        public override Color TotalUniqueViewsColor { get; } = Color.FromHex(_saturatedGreenHex);
        public override Color TotalClonesColor { get; } = Color.FromHex(_saturatedYellowHex);
        public override Color TotalUniqueClonesColor { get; } = Color.FromHex(_saturatedPinkHex);

        public override Color ChartAxisTextColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color ChartAxisLineColor { get; } = Color.FromHex(_primaryTextHex);

        //Components
        //Splash
        public override Color SplashScreenStatusColor { get; } = Color.FromHex(_primaryTextHex);

        //Icons
        public override Color IconColor { get; } = Color.FromHex(_buttonTextColor);
        public override Color IconPrimaryColor { get; } = Color.FromHex(_primaryTealHex);

        //Buttons
        public override Color ButtonTextColor { get; } = Color.FromHex(_buttonTextColor);
        public override Color ButtonBackgroundColor { get; } = Color.FromHex(_primaryTealHex);

        //Indicators
        public override Color RefreshControlColor { get; } = Color.FromHex(_primaryDarkTealHex);

        //Card Stats Color
        public override Color CardStarsStatsTextColor { get; } = Color.FromHex(_accentYellowHex);
        public override Color CardStarsStatsIconColor { get; } = Color.FromHex(_accentYellowHex);
        public override Color CardForksStatsTextColor { get; } = Color.FromHex(_primaryTealHex);
        public override Color CardForksStatsIconColor { get; } = Color.FromHex(_primaryTealHex);
        public override Color CardIssuesStatsTextColor { get; } = Color.FromHex(_accentOrangeHex);
        public override Color CardIssuesStatsIconColor { get; } = Color.FromHex(_accentOrangeHex);
        public override Color CardViewsStatsTextColor { get; } = Color.FromHex(_saturatedLightBlueHex);
        public override Color CardViewsStatsIconColor { get; } = Color.FromHex(_saturatedLightBlueHex);
        public override Color CardClonesStatsTextColor { get; } = Color.FromHex(_saturatedYellowHex);
        public override Color CardClonesStatsIconColor { get; } = Color.FromHex(_saturatedYellowHex);
        public override Color CardUniqueViewsStatsTextColor { get; } = Color.FromHex(_saturatedGreenHex);
        public override Color CardUniqueViewsStatsIconColor { get; } = Color.FromHex(_saturatedGreenHex);
        public override Color CardUniqueClonesStatsTextColor { get; } = Color.FromHex(_saturatedPinkHex);
        public override Color CardUniqueClonesStatsIconColor { get; } = Color.FromHex(_saturatedPinkHex);
        public override Color CardTrendingStatsColor { get; } = Color.FromHex(_primaryTealHex);

        //Trends Settings Component
        public override Color TrendsChartSettingsLabelTextColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color TrendsChartSettingsBorderColor { get; } = Color.FromHex(_black12PercentBlend);
        public override Color TrendsChartSettingsFontColor { get; } = Color.FromHex(_primaryTextHex);
        public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromHex(_primaryTealHex);

        public override Color GitTrendsImageBackgroundColor { get; } = Color.White;
    }
}
