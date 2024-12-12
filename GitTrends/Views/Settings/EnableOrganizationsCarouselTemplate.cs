using System.ComponentModel;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using static GitTrends.MauiService;
using ScrollView = Microsoft.Maui.Controls.ScrollView;

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
				.Margin(6, 12),

			new Image()
				.Row(EnableOrganizationsGrid.Row.Image)
				.Margin(24, 12).Aspect(Aspect.AspectFit)
				.Bind(Image.IsVisibleProperty,
					getter: static (IncludeOrganizationsCarouselModel vm) => vm.ImageSource,
					convert: source => source is not null,
					mode: BindingMode.OneTime)
				.Bind(Image.SourceProperty,
					getter: static (IncludeOrganizationsCarouselModel vm) => vm.ImageSource,
					mode: BindingMode.OneTime),

			new TitleLabel()
				.Row(EnableOrganizationsGrid.Row.Title)
				.Bind(Label.TextProperty,
					getter: static (IncludeOrganizationsCarouselModel vm) => vm.Title,
					mode: BindingMode.OneTime),

			IsSmallScreen
				? new ScrollView
				{
					Margin = 0,
					Padding = 0,
					Content = new DescriptionLabel()
						.Bind(Label.TextProperty,
							getter: static (IncludeOrganizationsCarouselModel vm) => vm.Text,
							mode: BindingMode.OneTime)
				}.Row(EnableOrganizationsGrid.Row.Description)
				: new DescriptionLabel()
					.Row(EnableOrganizationsGrid.Row.Description)
					.Bind(Label.TextProperty,
						getter: static (IncludeOrganizationsCarouselModel vm) => vm.Text,
						mode: BindingMode.OneTime),

			new GitHubButton(deviceInfo, SettingsPageAutomationIds.GitHubButton, SettingsPageConstants.ManageOrganizations)
				{
					IsVisible = false,
					IsEnabled = false
				}
				.Row(EnableOrganizationsGrid.Row.GitHubButton)
				.Bind(GitHubButton.CommandProperty,
					nameof(SettingsViewModel.OpenGitTrendsOrganizationBrowserCommand),
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(SettingsViewModel)),
					mode: BindingMode.OneTime)
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
		const int _chartVideoHeight = 1080;
		const int _chartVideoWidth = 1350;

		public MediaElementContentView()
		{
			Shadow = new Shadow
			{
				Brush = Colors.Black,
				Offset = new Point(5, 5),
				Radius = 10,
				Opacity = 0.2f
			};

			Content = new MediaElement
			{
				Background = null,
				ShouldAutoPlay = true,
				ShouldShowPlaybackControls = false,
				ShouldLoopPlayback = true,
				Volume = 0.0,
				Margin = 0,
				HeightRequest = 250, // Assign any value to MediaElement.HeightRequest; workaround to ensure MediaElement is inflated on iOS
				WidthRequest = 250 // Assign any value to MediaElement.WidthRequest; workaround to ensure MediaElement is inflated on iOS
			}.Center()
				.Bind(MediaElement.SourceProperty,
					getter: static (IncludeOrganizationsCarouselModel vm) => vm.VideoSource,
					convert: videoSource => MediaSource.FromResource(videoSource))
				.Bind(IsVisibleProperty,
					getter: static (IncludeOrganizationsCarouselModel vm) => vm.VideoSource,
					convert: source => source is not null);

			PropertyChanged += HandlePropertyChanged;
		}

		static void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			ArgumentNullException.ThrowIfNull(sender);

			var contentView = (MediaElementContentView)sender;

			// Ensure both Width + Height have been initialized 
			if ((e.PropertyName == HeightProperty.PropertyName
				|| e.PropertyName == WidthProperty.PropertyName)
				&& contentView.Height is not -1
				&& contentView.Width is not -1)
			{
				var mediaElement = (MediaElement)(contentView.Content ?? throw new InvalidOperationException($"{nameof(ContentView)}.{nameof(Content)} must be set to a MediaElement in the Constructor"));

				mediaElement.WidthRequest = contentView.Width - contentView.Padding.HorizontalThickness;
				mediaElement.HeightRequest = mediaElement.WidthRequest / _chartVideoWidth * _chartVideoHeight;
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