using Sharpnado.MaterialFrame;

namespace GitTrends;

public sealed class DarkTheme : BaseTheme
{
	public const string PageBackgroundColorHex = "#121212";

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

	//Surface Elevations
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
	public override Color NavigationBarBackgroundColor { get; } = Color.FromArgb(_toolbarSurfaceHex);
	public override Color NavigationBarTextColor { get; } = Color.FromArgb(_toolbarTextHex);

	public override Color PageBackgroundColor { get; } = Color.FromArgb(PageBackgroundColorHex);
	public override Color PageBackgroundColor_85Opacity { get; } = Color.FromArgb(PageBackgroundColorHex).MultiplyAlpha(0.85f);

	//Text
	public override Color PrimaryTextColor { get; } = Color.FromArgb(_primaryTextHex);
	public override Color TextColor { get; } = Color.FromArgb(_textHex);

	//Chart
	public override Color TotalViewsColor { get; } = Color.FromArgb(_saturatedLightBlueHex);
	public override Color TotalUniqueViewsColor { get; } = Color.FromArgb(_saturatedIndigoHex);
	public override Color TotalClonesColor { get; } = Color.FromArgb(_saturatedYellowHex);
	public override Color TotalUniqueClonesColor { get; } = Color.FromArgb(_saturatedPinkHex);

	public override Color ChartAxisTextColor { get; } = Color.FromArgb(_primaryTextHex);
	public override Color ChartAxisLineColor { get; } = Color.FromArgb(_primaryTextHex);

	//Components
	//Splash
	public override Color SplashScreenStatusColor { get; } = Color.FromArgb(_offWhite);

	//Icons
	public override Color IconColor { get; } = Color.FromArgb(_primaryTealHex);
	public override Color IconPrimaryColor { get; } = Color.FromArgb(_lightPrimaryTealHex);

	//Buttons
	public override Color ButtonTextColor { get; } = Color.FromArgb(_lightPrimaryTealHex);
	public override Color ButtonBackgroundColor { get; } = Color.FromArgb(_buttonTextColor);

	//Indicators
	public override Color ActivityIndicatorColor { get; } = Color.FromArgb(_lightPrimaryTealHex);
	public override Color PullToRefreshColor { get; } = DeviceInfo.Platform == DevicePlatform.iOS
														? Color.FromArgb(_lightPrimaryTealHex)
														: Color.FromArgb(_toolbarSurfaceHex);

	//Card
	public override Color CardSurfaceColor { get; } = Color.FromArgb(_cardSurfaceHex);
	public override Color CardBorderColor { get; } = Color.FromArgb(_cardSurfaceHex);

	public override Color SeparatorColor { get; } = Color.FromArgb(_white12PercentBlend);

	//Card Stats Color
	public override Color CardStarsStatsTextColor { get; } = Color.FromArgb(_lightAccentYellowHex);
	public override Color CardStarsStatsIconColor { get; } = Color.FromArgb(_lightAccentYellowHex);
	public override Color CardForksStatsTextColor { get; } = Color.FromArgb(_lightPrimaryTealHex);
	public override Color CardForksStatsIconColor { get; } = Color.FromArgb(_lightPrimaryTealHex);
	public override Color CardIssuesStatsTextColor { get; } = Color.FromArgb(_lightAccentOrangeHex);
	public override Color CardIssuesStatsIconColor { get; } = Color.FromArgb(_lightAccentOrangeHex);
	public override Color CardViewsStatsTextColor { get; } = Color.FromArgb(_saturatedLightBlueHex);
	public override Color CardViewsStatsIconColor { get; } = Color.FromArgb(_saturatedLightBlueHex);
	public override Color CardClonesStatsTextColor { get; } = Color.FromArgb(_saturatedYellowHex);
	public override Color CardClonesStatsIconColor { get; } = Color.FromArgb(_saturatedYellowHex);
	public override Color CardUniqueViewsStatsTextColor { get; } = Color.FromArgb(_saturatedIndigoHex);
	public override Color CardUniqueViewsStatsIconColor { get; } = Color.FromArgb(_saturatedIndigoHex);
	public override Color CardUniqueClonesStatsTextColor { get; } = Color.FromArgb(_saturatedPinkHex);
	public override Color CardUniqueClonesStatsIconColor { get; } = Color.FromArgb(_saturatedPinkHex);
	public override Color CardTrendingStatsColor { get; } = Color.FromArgb(_primaryTealHex);

	//Settings Components
	public override Color SettingsLabelTextColor { get; } = Color.FromArgb(_textHex);
	public override Color BorderButtonBorderColor { get; } = Color.FromArgb(_white12PercentBlend);
	public override Color BorderButtonFontColor { get; } = Color.FromArgb(_textHex);
	public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromArgb(_lightPrimaryTealHex);

	public override Color GitTrendsImageBackgroundColor { get; } = Color.FromArgb("4CADA2");

	public override Color GitHubButtonSurfaceColor { get; } = Color.FromArgb(_githubButtonSurfaceHex);
	public override Color GitHubHandleColor { get; } = Color.FromArgb(_primaryTextHex);

	public override Color PrimaryColor { get; } = Color.FromArgb(_primaryTealHex);

	public override Color CloseButtonTextColor { get; } = Color.FromArgb(_toolbarTextHex);
	public override Color CloseButtonBackgroundColor { get; } = Color.FromArgb(_cardSurfaceHex);

	public override Color PickerBorderColor { get; } = Color.FromArgb(_white12PercentBlend);

	public override string GitTrendsImageSource { get; } = "GitTrendsWhite";
	public override string DefaultProfileImageSource { get; } = "DefaultProfileImageWhite";
	public override string DefaultReferringSiteImageSource { get; } = "DefaultReferringSiteImage";

	public override MaterialFrame.Theme MaterialFrameTheme { get; } = MaterialFrame.Theme.Dark;
}