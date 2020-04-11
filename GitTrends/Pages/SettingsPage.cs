using System;
using GitTrends.Mobile.Shared;
using GitTrends.Views.Settings;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

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

            Content = CreateLayout();
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

        ScrollView CreateLayout()
        {

            var gitHubSettingsView = new GitHubSettingsView();
            var appSettingsView = new AppSettingsView();
            var trendsSettingsView = new TrendsChartSettingsView(_trendsChartSettingsService);
            var registerforNotificationsView = new RegisterForNotificationsView();

#if AppStore
            var versionNumberText = $"Version {VersionTracking.CurrentVersion}";
#elif RELEASE
            var versionNumberText = $"Version {VersionTracking.CurrentVersion} (Release)";
#elif DEBUG
            var versionNumberText = $"Version {VersionTracking.CurrentVersion} (Debug)";
#endif

            var createdByLabel = new CopyrightLabel(versionNumberText);
            createdByLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));
            createdByLabel.BindTapGesture(nameof(SettingsViewModel.CreatedByLabelTappedCommand));

            return new ScrollView()
            {
                Content = new StackLayout()
                {
                    Orientation = StackOrientation.Vertical,
                    Spacing = 0,
                    Children =
                    {
                        gitHubSettingsView,
                        appSettingsView,
                        trendsSettingsView.Margin(new Thickness(16,32,16,0)),
                        createdByLabel.Margin(new Thickness(0,24,0,32))

                    }
                }
            };
        }

        class CopyrightLabel : Label
        {
            public CopyrightLabel(in string versionText)
            {
                AutomationId = SettingsPageAutomationIds.CreatedByLabel;
                LineBreakMode = LineBreakMode.WordWrap;
                VerticalOptions = LayoutOptions.EndAndExpand;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.End;
                FontSize = 12;

                LineHeight = 1.82;

                FormattedText = new FormattedString
                {
                    Spans =
                    {
                        new Span
                        {
                            FontSize = 12,
                            FontFamily = FontFamilyConstants.RobotoMedium,
                            Text = $"{versionText}\n"
                        },
                        new Span
                        {
                            FontSize = 12,
                            FontFamily = FontFamilyConstants.RobotoRegular,
                            Text = "Mobile App Created by Code Traveler LLC"
                        }
                    }
                };
            }
        }
    }
}
