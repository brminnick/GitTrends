using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using static GitTrends.MauiService;

namespace GitTrends;

class EnableOrganizationsCarouselTemplate(IDeviceInfo deviceInfo, CarouselView organizationsCarouselView) : DataTemplate(() => CreateItemsGrid(deviceInfo, organizationsCarouselView))
{
	static EnableOrganizationsGrid CreateItemsGrid(IDeviceInfo deviceInfo, CarouselView organizationsCarouselView) => new()
	{
		Children =
		{
			new MediaElementContentView()
				.Row(EnableOrganizationsGrid.Row.Image)
				.Center()
				.Margin(24, 12),

			new Image()
				.Row(EnableOrganizationsGrid.Row.Image)
				.Margin(24, 12).Aspect(Aspect.AspectFit)
				.Bind(Image.IsVisibleProperty,
					nameof(IncludeOrganizationsCarouselModel.ImageSource),
					convert: (ImageSource? source) => source is not null)
				.Bind(Image.SourceProperty,
					nameof(IncludeOrganizationsCarouselModel.ImageSource)),

			new TitleLabel()
				.Row(EnableOrganizationsGrid.Row.Title)
				.Bind(Label.TextProperty,
					nameof(IncludeOrganizationsCarouselModel.Title)),

			IsSmallScreen
				? new ScrollView
				{
					Margin = 0,
					Padding = 0,
					Content = new DescriptionLabel()
						.Bind(Label.TextProperty,
							nameof(IncludeOrganizationsCarouselModel.Text))
				}.Row(EnableOrganizationsGrid.Row.Description)
				: new DescriptionLabel()
					.Row(EnableOrganizationsGrid.Row.Description)
					.Bind(Label.TextProperty,
						nameof(IncludeOrganizationsCarouselModel.Text)),

			new GitHubButton(deviceInfo, SettingsPageAutomationIds.GitHubButton, SettingsPageConstants.ManageOrganizations)
				{
					IsVisible = false,
					IsEnabled = false
				}
				.Row(EnableOrganizationsGrid.Row.GitHubButton)
				.Bind(GitHubButton.CommandProperty,
					nameof(SettingsViewModel.OpenGitTrendsOrganizationBrowserCommand),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(SettingsViewModel)))
				.Invoke(button =>
				{
					organizationsCarouselView.Scrolled += async (s, e) =>
					{
						var shouldDisplayButton = e.CenterItemIndex is 2
							&& organizationsCarouselView.Position is 2
							&& organizationsCarouselView.IsDragging is false;

						if (shouldDisplayButton)
						{
							button.Opacity = 0;
							button.IsVisible = true;
							await button.FadeTo(1, 2000, Easing.CubicIn);
							button.IsEnabled = true;
						}
						else
						{
							button.IsVisible = false;
						}
					};
				}),

			new BoxView()
				.Row(EnableOrganizationsGrid.Row.IndicatorView)
		}
	};

	sealed class MediaElementContentView : ContentView
	{
		public MediaElementContentView()
		{
			const int chartVideoHeight = 1080;
			const int chartVideoWidth = 1350;

			Content = new MediaElement
				{
					Background = null,
					ShouldAutoPlay = true,
					ShouldShowPlaybackControls = false,
					ShouldLoopPlayback = true,
					Volume = 0.0,
					Margin = 0,
				}.Center()
				.Bind(MediaElement.SourceProperty,
					nameof(IncludeOrganizationsCarouselModel.VideoSource),
					convert: (string? videoSource) => MediaSource.FromResource(videoSource))
				.Bind(IsVisibleProperty,
					nameof(IncludeOrganizationsCarouselModel.VideoSource),
					convert: (string? source) => source is not null)
				.Bind(HeightRequestProperty,
					source: this,
					getter: contentView => contentView.Width,
					convert: convertHeightToMatchRecordedVideoDimensions);

			// This ensures that black bars don't appear on the sides of the video due to being improperly scaled
			static double convertHeightToMatchRecordedVideoDimensions(double contentViewWidth)
			{
				return contentViewWidth is -1
					? -1
					: contentViewWidth / chartVideoWidth * chartVideoHeight;
			}
		}
	}

	sealed class TitleLabel : Label
	{
		public TitleLabel()
		{
			TextColor = Colors.White;

			FontSize = IsSmallScreen ? 28 : 34;
			FontFamily = FontFamilyConstants.RobotoBold;

			MaxLines = 1;
			LineHeight = 1.12;
			LineBreakMode = LineBreakMode.TailTruncation;

			AutomationId = SettingsPageAutomationIds.EnableOrangizationsCarouselTitle;

			Padding = new Thickness(24, 5);
		}
	}

	sealed class DescriptionLabel : Label
	{
		public DescriptionLabel()
		{
			TextColor = Colors.White;

			FontSize = 15;
			FontFamily = FontFamilyConstants.RobotoRegular;

			LineHeight = 1.021;

			VerticalTextAlignment = TextAlignment.Start;

			MaxLines = IsSmallScreen ? -1 : 3;
			LineBreakMode = IsSmallScreen ? LineBreakMode.WordWrap : LineBreakMode.TailTruncation;

			AutomationId = SettingsPageAutomationIds.EnableOrangizationsCarouselDescription;

			Padding = new Thickness(24, 5);
		}
	}
}