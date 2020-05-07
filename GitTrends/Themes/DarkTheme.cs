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

        const string _darkNavyBlueHex = "#0E1820";
        const string _lightNavyBlueHex = "#273D56";

        public override Color PageBackgroundColor { get; } = Color.FromHex(_backgroundSurfaceHex);
        public override Color PageBackgroundColor_75Opactity { get; } = Color.FromHex(_backgroundSurfaceHex).MultiplyAlpha(0.75);

        const string _offWhite = "#F0EFEF";

        public override Color NavigationBarBackgroundColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color NavigationBarTextColor { get; } = Color.FromHex(_offWhite);

        //Indicators
        public override Color ActivityIndicatorColor { get; } = Color.FromHex(_lightPrimaryTealHex);
        public override Color PullToRefreshColor { get; } = Device.RuntimePlatform is Device.iOS ? Color.FromHex(_lightPrimaryTealHex) : Color.FromHex(_toolbarSurfaceHex);

        public override Color TextColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color RefreshControlColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color TotalViewsColor { get; } = Color.FromHex(_darkestBlueHex);
        public override Color TotalUniqueViewsColor { get; } = Color.FromHex(_mediumBlueHex);
        public override Color TotalClonesColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TotalUniqueClonesColor { get; } = Color.FromHex(_lightNavyBlueHex);

        //Settings Components
        public override Color SettingsLabelTextColor { get; } = Color.FromHex(_textHex);
        public override Color BorderButtonBorderColor { get; } = Color.FromHex(_white12PercentBlend);
        public override Color BorderButtonFontColor { get; } = Color.FromHex(_textHex);
        public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromHex(_lightPrimaryTealHex);

        public override Color ButtonTextColor { get; } = Color.FromHex(_offWhite);
        public override Color ButtonBackgroundColor { get; } = Color.FromHex(_darkNavyBlueHex);

        public override Color SettingsLabelTextColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color SettingsButtonBorderColor { get; } = Color.FromHex(_darkBlueHex);
        public override Color SettingsButtonFontColor { get; } = Color.FromHex(_darkNavyBlueHex);
        public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromHex(_darkBlueHex);

        public override Color PrimaryColor { get; } = Color.FromHex(_primaryTealHex);

        public override string GitTrendsImageSource { get; } = "GitTrendsWhite";
        public override string DefaultProfileImageSource { get; } = "DefaultProfileImageWhite";
        public override string DefaultReferringSiteImageSource { get; } = "DefaultReferringSiteImage";
    }
}
