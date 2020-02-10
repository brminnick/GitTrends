using GitTrends.Mobile.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class SettingsPage : BaseContentPage<SettingsViewModel>
    {
        public SettingsPage(SettingsViewModel settingsViewModel, TrendsChartSettingsService trendsChartSettingsService) : base(PageTitles.SettingsPage, settingsViewModel)
        {
            ViewModel.GitHubLoginUrlRetrieved += HandleGitHubLoginUrlRetrieved;

            var stackLayout = new StackLayout
            {
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new GitHubSettingsView(),
                    new TrendsChartSettingsView(trendsChartSettingsService)
                }
            };

            Content = new ScrollView { Content = stackLayout };
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
    }
}
