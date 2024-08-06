using Sharpnado.MaterialFrame;

namespace GitTrends;

public sealed class LightTheme : BaseTheme
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
	const string _pageBackgroundColorHex = "#F1F1F1";
	const string _cardSurfaceHex = "#FFFFFF";
	const string _toolbarSurfaceHex = LightTealColorHex;
	const string _circleImageBackgroundHex = "#FFFFFF";
	const string _githubButtonSurfaceHex = "#231F20";

	//Graph Palette
	const string _saturatedLightBlueHex = "#2BC3BE";
	const string _saturatedIndigoHex = "#5D6AB1";
	const string _saturatedYellowHex = "#FFC534";
	const string _saturatedPinkHex = "#F2726F";

	//Blended Colors
	const string _black12PercentBlend = "#E0E0E0";

	//Surfaces
	public override Color NavigationBarBackgroundColor { get; } = Color.FromArgb(_toolbarSurfaceHex);
	public override Color NavigationBarTextColor { get; } = Color.FromArgb(_toolbarTextHex);

	public override Color PageBackgroundColor { get; } = Color.FromArgb(_pageBackgroundColorHex);
	public override Color PageBackgroundColor_85Opacity { get; } = Color.FromArgb(_pageBackgroundColorHex).MultiplyAlpha(0.85f);

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
	public override Color SplashScreenStatusColor { get; } = Color.FromArgb(_primaryTextHex);

	//Icons
	public override Color IconColor { get; } = Color.FromArgb(_iconColor);
	public override Color IconPrimaryColor { get; } = Color.FromArgb(_primaryTealHex);

	//Buttons
	public override Color ButtonTextColor { get; } = Color.FromArgb(_buttonTextColor);
	public override Color ButtonBackgroundColor { get; } = Color.FromArgb(_primaryTealHex);

	//Indicators
	public override Color ActivityIndicatorColor { get; } = Color.FromArgb(_primaryDarkTealHex);
	public override Color PullToRefreshColor { get; } = Color.FromArgb(_toolbarSurfaceHex);

	//Card
	public override Color CardSurfaceColor { get; } = Color.FromArgb(_cardSurfaceHex);
	public override Color CardBorderColor { get; } = Color.FromArgb(_black12PercentBlend);

	public override Color SeparatorColor { get; } = Color.FromArgb(_black12PercentBlend);

	//Card Stats Color
	public override Color CardStarsStatsTextColor { get; } = Color.FromArgb(_textHex);
	public override Color CardStarsStatsIconColor { get; } = Color.FromArgb(_accentYellowHex);
	public override Color CardForksStatsTextColor { get; } = Color.FromArgb(_textHex);
	public override Color CardForksStatsIconColor { get; } = Color.FromArgb(_primaryTealHex);
	public override Color CardIssuesStatsTextColor { get; } = Color.FromArgb(_textHex);
	public override Color CardIssuesStatsIconColor { get; } = Color.FromArgb(_accentOrangeHex);
	public override Color CardViewsStatsTextColor { get; } = Color.FromArgb(_textHex);
	public override Color CardViewsStatsIconColor { get; } = Color.FromArgb(_saturatedLightBlueHex);
	public override Color CardClonesStatsTextColor { get; } = Color.FromArgb(_textHex);
	public override Color CardClonesStatsIconColor { get; } = Color.FromArgb(_saturatedYellowHex);
	public override Color CardUniqueViewsStatsTextColor { get; } = Color.FromArgb(_textHex);
	public override Color CardUniqueViewsStatsIconColor { get; } = Color.FromArgb(_saturatedIndigoHex);
	public override Color CardUniqueClonesStatsTextColor { get; } = Color.FromArgb(_textHex);
	public override Color CardUniqueClonesStatsIconColor { get; } = Color.FromArgb(_saturatedPinkHex);
	public override Color CardTrendingStatsColor { get; } = Color.FromArgb(_primaryTealHex);

	//Settings Components
	public override Color SettingsLabelTextColor { get; } = Color.FromArgb(_primaryTextHex);
	public override Color BorderButtonBorderColor { get; } = Color.FromArgb(_black12PercentBlend);
	public override Color BorderButtonFontColor { get; } = Color.FromArgb(_primaryTextHex);
	public override Color TrendsChartSettingsSelectionIndicatorColor { get; } = Color.FromArgb(_primaryTealHex);

	public override Color GitTrendsImageBackgroundColor { get; } = Colors.White;

	public override Color GitHubButtonSurfaceColor { get; } = Color.FromArgb(_githubButtonSurfaceHex);

	public override Color GitHubHandleColor { get; } = Color.FromArgb(_textHex);

	public override Color PrimaryColor { get; } = Color.FromArgb(_primaryTealHex);

	public override Color CloseButtonTextColor { get; } = Color.FromArgb(_toolbarTextHex);
	public override Color CloseButtonBackgroundColor { get; } = Color.FromArgb(_toolbarSurfaceHex);

	public override Color PickerBorderColor { get; } = Colors.LightGray;

	public override string GitTrendsImageSource { get; } = "GitTrendsGreen";
	public override string DefaultProfileImageSource { get; } = "DefaultProfileImageGreen";
	public override string DefaultReferringSiteImageSource { get; } = "DefaultReferringSiteImage";

	public override MaterialFrame.Theme MaterialFrameTheme { get; } = MaterialFrame.Theme.Light;
}