using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using static GitTrends.MauiService;

namespace GitTrends;

class EnableOrganizationsCarouselTemplateSelector : DataTemplateSelector
{
	protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new EnableOrganizationsCarouselTemplate((IncludeOrganizationsCarouselModel)item, (CarouselView)container);

	sealed class EnableOrganizationsCarouselTemplate(IncludeOrganizationsCarouselModel includeOrganizationsModel, CarouselView organizationsCarouselView) 
		: DataTemplate(() => CreateItemsGrid(includeOrganizationsModel, organizationsCarouselView))
	{
		static EnableOrganizationsGrid CreateItemsGrid(IncludeOrganizationsCarouselModel includeOrganizationsModel, CarouselView organizationsCarouselView) => new()
		{
			Children =
			{
				includeOrganizationsModel.Url is not null
					? new MediaElement
						{
							Source = MediaSource.FromResource(VideoConstants.ChartVideoFileName),
							Background = null,
							ShouldAutoPlay = true,
							ShouldShowPlaybackControls = false,
							ShouldLoopPlayback = true,
							Volume = 0.0,
							Margin = 0,
						}.Center()
						.Margin(24, 12)
						.Row(EnableOrganizationsGrid.Row.Image)
					: new Image { Source = includeOrganizationsModel.ImageSource, Aspect = Aspect.AspectFit }.Margin(24, 12)
						.Row(EnableOrganizationsGrid.Row.Image),

				new TitleLabel(includeOrganizationsModel.Title)
					.Row(EnableOrganizationsGrid.Row.Title),

				IsSmallScreen
					? new ScrollView
					{
						Margin = 0,
						Padding = 0,
						Content = new DescriptionLabel(includeOrganizationsModel.Text)
					}.Row(EnableOrganizationsGrid.Row.Description)

					: new DescriptionLabel(includeOrganizationsModel.Text).Row(EnableOrganizationsGrid.Row.Description),

				new GitHubButton(SettingsPageAutomationIds.GitHubButton, SettingsPageConstants.ManageOrganizations) { IsVisible = false, IsEnabled = false }
					.Row(EnableOrganizationsGrid.Row.GitHubButton)
					.Bind(GitHubButton.CommandProperty, nameof(SettingsViewModel.OpenGitTrendsOrganizationBrowserCommand), source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(SettingsViewModel)))
					.Invoke(button =>
					{
						organizationsCarouselView.Scrolled += async (s, e) =>
						{
							var shouldDisplayButton = e.CenterItemIndex is 2
								&& organizationsCarouselView.Position is 2
								&& organizationsCarouselView.IsDragging is false;
							if(shouldDisplayButton)
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

		sealed class TitleLabel : Label
		{
			public TitleLabel(in string text)
			{
				Text = text;
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
			public DescriptionLabel(in string text)
			{
				Text = text;
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
}