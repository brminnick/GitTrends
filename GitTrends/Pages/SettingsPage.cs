using System.Collections;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        readonly DeepLinkingService _deepLinkingService;

        public SettingsPage(SettingsViewModel settingsViewModel,
                            TrendsChartSettingsService trendsChartSettingsService,
                            AnalyticsService analyticsService,
                            DeepLinkingService deepLinkingService) : base(PageTitles.SettingsPage, settingsViewModel, analyticsService)
        {
            _deepLinkingService = deepLinkingService;

            ViewModel.GitHubLoginUrlRetrieved += HandleGitHubLoginUrlRetrieved;

            var versionNumberLabel = new Label
            {
                Text = $"Version: {Xamarin.Essentials.VersionTracking.CurrentVersion}",
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

            var stackLayout = new StackLayout
            {
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new GitHubSettingsView(),
                    new TrendsChartSettingsView(trendsChartSettingsService),
                    versionNumberLabel,
                    createdByLabel
                }
            };

            Content = stackLayout;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ViewModel.IsAuthenticating = false;
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
