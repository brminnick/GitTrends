using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;


/* Unmerged change from project 'GitTrends(net8.0-android)'
Before:
namespace GitTrends
{
	public class GitTrendsOnboardingView(
		IDeviceInfo deviceInfo,
		IAnalyticsService analyticsService)
		: BaseOnboardingDataTemplate(
			OnboardingConstants.SkipText,
			deviceInfo,
			Color.FromArgb(BaseTheme.LightTealColorHex),
			0,
			() => new ImageView(),
			() => new(OnboardingConstants.GitTrendsPage_Title),
			() => new DescriptionBodyView(deviceInfo),
			analyticsService)
	{

		enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
		enum Column { Image, Description }

		sealed class ImageView : Image
		{
			public ImageView()
			{
				Source = "GitTrendsWhite";
				this.Center();
			}
		}

		sealed class DescriptionBodyView : ScrollView
		{
			public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

						new BodySvg(deviceInfo, "chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
						new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

						new BodySvg(deviceInfo, "megaphone.svg").Row(Row.Discover).Column(Column.Image),
						new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
					}
				};
			}
		}

		sealed class GitHubLogoLabel : Label
		{
			public GitHubLogoLabel()
			{
				Text = FontAwesomeBrandsConstants.GitHubOctocat;
				FontSize = 24;
				TextColor = Colors.White;
				FontFamily = FontFamilyConstants.FontAwesomeBrands;
				VerticalTextAlignment = TextAlignment.Center;
				HorizontalTextAlignment = TextAlignment.Center;
			}
After:
namespace GitTrends;

public class GitTrendsOnboardingView(
	IDeviceInfo deviceInfo,
	IAnalyticsService analyticsService)
	: BaseOnboardingDataTemplate(
		OnboardingConstants.SkipText,
		deviceInfo,
		Color.FromArgb(BaseTheme.LightTealColorHex),
		0,
		() => new ImageView(),
		() => new(OnboardingConstants.GitTrendsPage_Title),
		() => new DescriptionBodyView(deviceInfo),
		analyticsService)
{

	enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
	enum Column { Image, Description }

	sealed class ImageView : Image
	{
		public ImageView()
		{
			Source = "GitTrendsWhite";
			this.Center();
		}
	}

	sealed class DescriptionBodyView : ScrollView
	{
		public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

					new BodySvg(deviceInfo, "chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

					new BodySvg(deviceInfo, "megaphone.svg").Row(Row.Discover).Column(Column.Image),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
				}
			};
		}
	}

	sealed class GitHubLogoLabel : Label
	{
		public GitHubLogoLabel()
		{
			Text = FontAwesomeBrandsConstants.GitHubOctocat;
			FontSize = 24;
			TextColor = Colors.White;
			FontFamily = FontFamilyConstants.FontAwesomeBrands;
			VerticalTextAlignment = TextAlignment.Center;
			HorizontalTextAlignment = TextAlignment.Center;
*/

/* Unmerged change from project 'GitTrends(net8.0-ios)'
Before:
namespace GitTrends
{
	public class GitTrendsOnboardingView(
		IDeviceInfo deviceInfo,
		IAnalyticsService analyticsService)
		: BaseOnboardingDataTemplate(
			OnboardingConstants.SkipText,
			deviceInfo,
			Color.FromArgb(BaseTheme.LightTealColorHex),
			0,
			() => new ImageView(),
			() => new(OnboardingConstants.GitTrendsPage_Title),
			() => new DescriptionBodyView(deviceInfo),
			analyticsService)
	{

		enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
		enum Column { Image, Description }

		sealed class ImageView : Image
		{
			public ImageView()
			{
				Source = "GitTrendsWhite";
				this.Center();
			}
		}

		sealed class DescriptionBodyView : ScrollView
		{
			public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

						new BodySvg(deviceInfo, "chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
						new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

						new BodySvg(deviceInfo, "megaphone.svg").Row(Row.Discover).Column(Column.Image),
						new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
					}
				};
			}
		}

		sealed class GitHubLogoLabel : Label
		{
			public GitHubLogoLabel()
			{
				Text = FontAwesomeBrandsConstants.GitHubOctocat;
				FontSize = 24;
				TextColor = Colors.White;
				FontFamily = FontFamilyConstants.FontAwesomeBrands;
				VerticalTextAlignment = TextAlignment.Center;
				HorizontalTextAlignment = TextAlignment.Center;
			}
After:
namespace GitTrends;

public class GitTrendsOnboardingView(
	IDeviceInfo deviceInfo,
	IAnalyticsService analyticsService)
	: BaseOnboardingDataTemplate(
		OnboardingConstants.SkipText,
		deviceInfo,
		Color.FromArgb(BaseTheme.LightTealColorHex),
		0,
		() => new ImageView(),
		() => new(OnboardingConstants.GitTrendsPage_Title),
		() => new DescriptionBodyView(deviceInfo),
		analyticsService)
{

	enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
	enum Column { Image, Description }

	sealed class ImageView : Image
	{
		public ImageView()
		{
			Source = "GitTrendsWhite";
			this.Center();
		}
	}

	sealed class DescriptionBodyView : ScrollView
	{
		public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

					new BodySvg(deviceInfo, "chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

					new BodySvg(deviceInfo, "megaphone.svg").Row(Row.Discover).Column(Column.Image),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
				}
			};
		}
	}

	sealed class GitHubLogoLabel : Label
	{
		public GitHubLogoLabel()
		{
			Text = FontAwesomeBrandsConstants.GitHubOctocat;
			FontSize = 24;
			TextColor = Colors.White;
			FontFamily = FontFamilyConstants.FontAwesomeBrands;
			VerticalTextAlignment = TextAlignment.Center;
			HorizontalTextAlignment = TextAlignment.Center;
*/

/* Unmerged change from project 'GitTrends(net8.0-maccatalyst)'
Before:
namespace GitTrends
{
	public class GitTrendsOnboardingView(
		IDeviceInfo deviceInfo,
		IAnalyticsService analyticsService)
		: BaseOnboardingDataTemplate(
			OnboardingConstants.SkipText,
			deviceInfo,
			Color.FromArgb(BaseTheme.LightTealColorHex),
			0,
			() => new ImageView(),
			() => new(OnboardingConstants.GitTrendsPage_Title),
			() => new DescriptionBodyView(deviceInfo),
			analyticsService)
	{

		enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
		enum Column { Image, Description }

		sealed class ImageView : Image
		{
			public ImageView()
			{
				Source = "GitTrendsWhite";
				this.Center();
			}
		}

		sealed class DescriptionBodyView : ScrollView
		{
			public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

						new BodySvg(deviceInfo, "chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
						new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

						new BodySvg(deviceInfo, "megaphone.svg").Row(Row.Discover).Column(Column.Image),
						new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
					}
				};
			}
		}

		sealed class GitHubLogoLabel : Label
		{
			public GitHubLogoLabel()
			{
				Text = FontAwesomeBrandsConstants.GitHubOctocat;
				FontSize = 24;
				TextColor = Colors.White;
				FontFamily = FontFamilyConstants.FontAwesomeBrands;
				VerticalTextAlignment = TextAlignment.Center;
				HorizontalTextAlignment = TextAlignment.Center;
			}
After:
namespace GitTrends;

public class GitTrendsOnboardingView(
	IDeviceInfo deviceInfo,
	IAnalyticsService analyticsService)
	: BaseOnboardingDataTemplate(
		OnboardingConstants.SkipText,
		deviceInfo,
		Color.FromArgb(BaseTheme.LightTealColorHex),
		0,
		() => new ImageView(),
		() => new(OnboardingConstants.GitTrendsPage_Title),
		() => new DescriptionBodyView(deviceInfo),
		analyticsService)
{

	enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
	enum Column { Image, Description }

	sealed class ImageView : Image
	{
		public ImageView()
		{
			Source = "GitTrendsWhite";
			this.Center();
		}
	}

	sealed class DescriptionBodyView : ScrollView
	{
		public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

					new BodySvg(deviceInfo, "chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

					new BodySvg(deviceInfo, "megaphone.svg").Row(Row.Discover).Column(Column.Image),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
				}
			};
		}
	}

	sealed class GitHubLogoLabel : Label
	{
		public GitHubLogoLabel()
		{
			Text = FontAwesomeBrandsConstants.GitHubOctocat;
			FontSize = 24;
			TextColor = Colors.White;
			FontFamily = FontFamilyConstants.FontAwesomeBrands;
			VerticalTextAlignment = TextAlignment.Center;
			HorizontalTextAlignment = TextAlignment.Center;
*/
namespace GitTrends;

public class GitTrendsOnboardingView(
	IDeviceInfo deviceInfo,
	IAnalyticsService analyticsService)
	: BaseOnboardingDataTemplate(
		OnboardingConstants.SkipText,
		deviceInfo,
		Color.FromArgb(BaseTheme.LightTealColorHex),
		0,
		() => new ImageView(),
		() => new(OnboardingConstants.GitTrendsPage_Title),
		() => new DescriptionBodyView(deviceInfo),
		analyticsService)
{

	enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
	enum Column { Image, Description }

	sealed class ImageView : Image
	{
		public ImageView()
		{
			Source = "GitTrendsWhite";
			this.Center();
		}
	}

	sealed class DescriptionBodyView : ScrollView
	{
		public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

					new BodySvg(deviceInfo, "chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_MonitorGitHubRepos).TextTop().Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

					new BodySvg(deviceInfo, "megaphone.svg").Row(Row.Discover).Column(Column.Image),
					new BodyLabel(OnboardingConstants.GitTrendsPage_Body_DiscoverReferringSites).Row(Row.Discover).Column(Column.Description),
				}
			};
		}
	}

	sealed class GitHubLogoLabel : Label
	{
		public GitHubLogoLabel()
		{
			Text = FontAwesomeBrandsConstants.GitHubOctocat;
			FontSize = 24;
			TextColor = Colors.White;
			FontFamily = FontFamilyConstants.FontAwesomeBrands;
			VerticalTextAlignment = TextAlignment.Center;
			HorizontalTextAlignment = TextAlignment.Center;
		}
	}
}