using System;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        readonly TrendsChartSettingsService _trendsChartSettingsService;

        public SettingsPage(SettingsViewModel settingsViewModel,
                            TrendsChartSettingsService trendsChartSettingsService,
                            AnalyticsService analyticsService) : base(PageTitles.SettingsPage, settingsViewModel, analyticsService)
        {
            _trendsChartSettingsService = trendsChartSettingsService;

            ViewModel.GitHubLoginUrlRetrieved += HandleGitHubLoginUrlRetrieved;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ViewModel.IsAuthenticating = false;
        }

        protected override void HandlePageSizeChanged(object sender, EventArgs e)
        {
            base.HandlePageSizeChanged(sender, e);

            Content = CreateLayout(DeviceDisplay.MainDisplayInfo.Height > DeviceDisplay.MainDisplayInfo.Width);
        }

        RelativeLayout CreateLayout(bool isPortraitOrientation)
        {
            var gitHubSettingsView = new GitHubSettingsView();
            var trendsSettingsView = new TrendsChartSettingsView(_trendsChartSettingsService);

            var versionNumberLabel = new Label
            {
#if AppStore
                Text = $"Version: {VersionTracking.CurrentVersion}",
#elif RELEASE
                Text = $"Version: {VersionTracking.CurrentVersion} (Release)",
#elif DEBUG
                Text = $"Version: {VersionTracking.CurrentVersion} (Debug)",
#else
                throw new NotSupportedException()
#endif
                HorizontalTextAlignment = TextAlignment.Start,
                Opacity = 0.75,
                Margin = new Thickness(2, 5, 0, 0),
                FontSize = 14
            };
            versionNumberLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));

            var createdByLabel = new Label
            {
                AutomationId = SettingsPageAutomationIds.CreatedByLabel,
                Margin = new Thickness(0, 5),
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.End,
                Text = "Mobile App Created by Code Traveler LLC",
                FontSize = 12,
            };
            createdByLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));
            createdByLabel.BindTapGesture(nameof(SettingsViewModel.CreatedByLabelTappedCommand));

            var relativeLayout = new RelativeLayout
            {
                Margin = isPortraitOrientation ? new Thickness(20, 20, 20, 30) : new Thickness(20, 20, 20, 0)
            };

            relativeLayout.Children.Add(createdByLabel,
                xConstraint: Constraint.RelativeToParent(parent => parent.Width / 2 - GetWidth(parent, createdByLabel) / 2),
                yConstraint: Constraint.RelativeToParent(parent => parent.Height - GetHeight(parent, createdByLabel) - 10));

            relativeLayout.Children.Add(gitHubSettingsView,
                //Keep at left of the screen
                xConstraint: Constraint.Constant(0),
                //Keep at top of the screen
                yConstraint: Constraint.Constant(0),
                //Portrait: Full width; Landscape: Half of the screen
                widthConstraint: isPortraitOrientation
                                    ? Constraint.RelativeToParent(parent => parent.Width)
                                    : Constraint.RelativeToParent(parent => parent.Width / 2),
                //Portrait: Half height; Landscape: Full height
                heightConstraint: isPortraitOrientation
                                    ? Constraint.RelativeToParent(parent => parent.Height / 2)
                                    : Constraint.RelativeToParent(parent => parent.Height));

            relativeLayout.Children.Add(trendsSettingsView,
                //Portrait: Place under GitHubSettingsView; Landscape: Place to the right of GitHubSettingsView
                xConstraint: isPortraitOrientation
                                ? Constraint.Constant(0)
                                : Constraint.RelativeToParent(parent => parent.Width / 2),
                //Portrait: Place under GitHubSettingsView; Landscape: Place on the top
                yConstraint: isPortraitOrientation
                                ? Constraint.RelativeToView(gitHubSettingsView, (parent, view) => view.Y + view.Height + 10)
                                : Constraint.Constant(0),
                //Portrait: Full width; Landscape: Half of the screen
                widthConstraint: isPortraitOrientation
                                    ? Constraint.RelativeToParent(parent => parent.Width)
                                    : Constraint.RelativeToParent(parent => parent.Width / 2));

            relativeLayout.Children.Add(versionNumberLabel,
                xConstraint: Constraint.RelativeToView(trendsSettingsView, (parent, view) => view.X),
                yConstraint: Constraint.RelativeToView(trendsSettingsView, (parent, view) => view.Y + view.Height + 10));

            return relativeLayout;
        }

        async void HandleGitHubLoginUrlRetrieved(object sender, string? loginUrl)
        {
            if (!string.IsNullOrWhiteSpace(loginUrl))
                await OpenBrowser(loginUrl);
            else
                await DisplayAlert("Error", "Couldn't connect to GitHub Login. Check your internet connection and try again", "OK");
        }
    }
}
