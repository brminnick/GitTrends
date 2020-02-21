using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        readonly DeepLinkingService _deepLinkingService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;

        public SettingsPage(SettingsViewModel settingsViewModel,
                            TrendsChartSettingsService trendsChartSettingsService,
                            AnalyticsService analyticsService,
                            DeepLinkingService deepLinkingService) : base(PageTitles.SettingsPage, settingsViewModel, analyticsService)
        {
            _trendsChartSettingsService = trendsChartSettingsService;
            _deepLinkingService = deepLinkingService;

            ViewModel.GitHubLoginUrlRetrieved += HandleGitHubLoginUrlRetrieved;

            Content = CreateLayout();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ViewModel.IsAuthenticating = false;
        }

        protected override void HandleDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            Content = CreateLayout();

            base.HandleDisplayInfoChanged(sender, e);
        }

        RelativeLayout CreateLayout()
        {
            var gitHubSettingsView = new GitHubSettingsView();
            var trendsSettingsView = new TrendsChartSettingsView(_trendsChartSettingsService);

            var versionNumberLabel = new Label
            {
                Text = $"Version: {VersionTracking.CurrentVersion}",
                HorizontalTextAlignment = TextAlignment.Start,
                Opacity = 0.75,
                Margin = new Thickness(2, 5, 0, 0),
                FontSize = 14
            };
            versionNumberLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));

            var createdByLabel = new Label
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.End,
                Text = "Mobile App Created by Code Traveler LLC",
                FontSize = 12,
            };
            createdByLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));
            createdByLabel.GestureRecognizers.Add(new TapGestureRecognizer { Command = new AsyncCommand(CreatedByLabelTapped) });

            var relativeLayout = new RelativeLayout { Margin = new Thickness(20) };

            relativeLayout.Children.Add(gitHubSettingsView,
                //Portrait: Center GitHubSettingsView; Landscape: Left-justify  GitHubSettingsView
                xConstraint: DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Portrait
                                ? null : Constraint.Constant(0),
                //Keep at top of the screen
                yConstraint: Constraint.Constant(0),
                //Portrait: Full width; Landscape: Half of the screen
                widthConstraint: DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Portrait
                                    ? Constraint.RelativeToParent(parent => parent.Width)
                                    : Constraint.RelativeToParent(parent => parent.Width / 2));

            relativeLayout.Children.Add(trendsSettingsView,
                //Portrait: Place under GitHubSettingsView; Landscape: Place to the right of GitHubSettingsView
                xConstraint: DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Portrait
                                ? Constraint.Constant(0)
                                : Constraint.RelativeToParent(parent => parent.Width / 2),
                //Portrait: Place under GitHubSettingsView; Landscape: Place on the top
                yConstraint: DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Portrait
                                ? Constraint.RelativeToView(gitHubSettingsView, (parent, view) => view.Y + GetHeight(parent, gitHubSettingsView) + 10)
                                : Constraint.Constant(0),
                //Portrait: Full width; Landscape: Half of the screen
                widthConstraint: DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Portrait
                                    ? Constraint.RelativeToParent(parent => parent.Width)
                                    : Constraint.RelativeToParent(parent => parent.Width / 2));

            relativeLayout.Children.Add(versionNumberLabel,
                xConstraint: Constraint.RelativeToView(trendsSettingsView, (parent, view) => view.X),
                yConstraint: Constraint.RelativeToView(trendsSettingsView, (parent, view) => view.Y + GetHeight(parent, trendsSettingsView) + 10));

            relativeLayout.Children.Add(createdByLabel,
                //Portrait: Center the Label on the screen; Landscape: Center the Label on the right-half of the screen
                xConstraint: DeviceDisplay.MainDisplayInfo.Orientation is DisplayOrientation.Portrait
                                ? Constraint.RelativeToParent(parent => parent.Width / 2 - GetWidth(parent, createdByLabel) / 2)
                                : Constraint.RelativeToParent(parent => parent.Width * 3 / 4 - GetWidth(parent, createdByLabel) / 2),
                yConstraint: Constraint.RelativeToParent(parent => parent.Height - GetHeight(parent, createdByLabel) - 20));

            return relativeLayout;
        }

        async void HandleGitHubLoginUrlRetrieved(object sender, string? loginUrl)
        {
            if (!string.IsNullOrWhiteSpace(loginUrl))
                await OpenBrowser(loginUrl);
            else
                await DisplayAlert("Error", "Couldn't connect to GitHub Login. Check your internet connection and try again", "OK");
        }

        Task CreatedByLabelTapped()
        {
            AnalyticsService.Track("CreatedBy Label Tapped");
            return _deepLinkingService.OpenApp("twitter://user?id=3418408341", "https://twitter.com/intent/user?user_id=3418408341");
        }
    }
}
