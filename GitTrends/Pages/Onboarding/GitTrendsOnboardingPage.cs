using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends;

public class GitTrendsOnboardingPage : BaseOnboardingContentPage
{
	public GitTrendsOnboardingPage(IDeviceInfo deviceInfo,
									IMainThread mainThread,
									IAnalyticsService analyticsService,
									MediaElementService mediaElementService)
		: base(OnboardingConstants.SkipText, deviceInfo, Color.FromHex(BaseTheme.LightTealColorHex), mainThread, 0, analyticsService, mediaElementService)
	{
	}

	enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
	enum Column { Image, Description }

	protected override View CreateImageView() => new Image
	{
		Source = "GitTrendsWhite"
	}.Center();

	protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.GitTrendsPage_Title);

	protected override View CreateDescriptionBodyView() => new ScrollView
	{
		Content = new Grid
		{
			RowSpacing = 14,

			RowDefinitions = Rows.Define(
				(Row.Title, Auto),
				(Row.Connect, 24),
				(Row.MonitorImage, 24),
				(Row.MonitorDescription, 2),
				(Row.Discover, 24)),

			ColumnDefinitions = Columns.Define(
				(Column.Image, 56),
				(Column.Description, Star)),

			Children =
			{
				new BodyLabel(OnboardingConstants.GitTrendsPage_Body_GitTrendsHelps).Row(Row.Title).ColumnSpan(All<Column>()),

				new GitHubLogoLabel().Row(Row.Connect).Column(Column.Image),
				new BodyLabel(GitHubLoginButtonConstants.ConnectToGitHub).Row(Row.Connect).Column(Column.Description),

				new BodySvg("chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
				new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

				new BodySvg("megaphone.svg").Row(Row.Discover).Column(Column.Image),
				new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
			}
		}
	};

	class GitHubLogoLabel : Label
	{
		public GitHubLogoLabel()
		{
			Text = FontAwesomeBrandsConstants.GitHubOctocat;
			FontSize = 24;
			TextColor = Color.White;
			FontFamily = FontFamilyConstants.FontAwesomeBrands;
			VerticalTextAlignment = TextAlignment.Center;
			HorizontalTextAlignment = TextAlignment.Center;
		}
	}
}