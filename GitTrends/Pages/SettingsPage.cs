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

        protected override void OnAppearing()
        {
            ViewModel.IsAuthenticating = false;

            base.OnAppearing();
        }

        async void HandleGitHubLoginUrlRetrieved(object sender, string loginUrl) => await OpenBrowser(loginUrl);
    }
}
