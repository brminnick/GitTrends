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
                            NotificationService notificationService,
                            TrendsChartSettingsService trendsChartSettingsService,
                            AnalyticsService analyticsService) : base(settingsViewModel, analyticsService, PageTitles.SettingsPage)
        {
            _trendsChartSettingsService = trendsChartSettingsService;
            notificationService.RegisterForNotificationsCompleted += HandleRegisterForNotificationsCompleted;

            Content = CreateLayout(DeviceDisplay.MainDisplayInfo.Height > DeviceDisplay.MainDisplayInfo.Width);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ViewModel.IsAuthenticating = false;
        }

        void HandleRegisterForNotificationsCompleted(object sender, (bool IsSuccessful, string ErrorMessage) result)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (result.IsSuccessful)
                    await DisplayAlert("Success", "Device Registered for Notificaitons", "OK");
                else
                    await DisplayAlert("Registration Failed", result.ErrorMessage, "OK");
            });
        }

        RelativeLayout CreateLayout(bool isPortraitOrientation)
        {
            var gitHubSettingsView = new GitHubSettingsView();
            var trendsSettingsView = new TrendsChartSettingsView(_trendsChartSettingsService);
            var registerforNotificationsView = new RegisterForNotificationsView();

#if AppStore
            var versionNumberText = $"Version {VersionTracking.CurrentVersion}";
#elif RELEASE
            var versionNumberText = $"Version {VersionTracking.CurrentVersion} (Release)";
#elif DEBUG
            var versionNumberText = $"Version {VersionTracking.CurrentVersion} (Debug)";
#endif

            var createdByLabel = new Label
            {
                AutomationId = SettingsPageAutomationIds.CreatedByLabel,
                Margin = new Thickness(0, 5),
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.End,
                Text = $"{versionNumberText}\nMobile App Created by Code Traveler LLC",
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

            relativeLayout.Children.Add(registerforNotificationsView,
                xConstraint: Constraint.RelativeToView(trendsSettingsView, (parent, view) => view.X),
                yConstraint: Constraint.RelativeToView(trendsSettingsView, (parent, view) => view.Y + view.Height + 20),
                widthConstraint: Constraint.RelativeToView(trendsSettingsView, (parent, view) => view.Width));

            return relativeLayout;
        }
    }
}
