#if !AppStore
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Syncfusion.SfChart.XForms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class UITestBackdoorService
    {
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly TrendsChartSettingsService _trendsChartSettingsService;

        public UITestBackdoorService(GitHubAuthenticationService gitHubAuthenticationService,
                                        GitHubGraphQLApiService gitHubGraphQLApiService,
                                        TrendsChartSettingsService trendsChartSettingsService)
        {
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _trendsChartSettingsService = trendsChartSettingsService;
        }

        public async Task SetGitHubUser(string token)
        {
            await _gitHubAuthenticationService.SaveGitHubToken(new GitHubToken(token, string.Empty, "Bearer")).ConfigureAwait(false);

            var (alias, name, avatarUri) = await _gitHubGraphQLApiService.GetCurrentUserInfo().ConfigureAwait(false);

            GitHubAuthenticationService.Alias = alias;
            GitHubAuthenticationService.AvatarUrl = avatarUri.ToString();
            GitHubAuthenticationService.Name = name;
        }

        public Task TriggerPullToRefresh() => MainThread.InvokeOnMainThreadAsync(() => GetVisibleRefreshView().IsRefreshing = true);

        public IReadOnlyList<T> GetVisibleCollection<T>() => GetVisibleCollection().Cast<T>().ToList();

        public IEnumerable GetVisibleCollection()
        {
            var collectionView = (CollectionView)GetVisibleRefreshView().Content;
            return collectionView.ItemsSource;
        }

        public TrendsChartOption GetCurrentTrendsChartOption() => _trendsChartSettingsService.CurrentTrendsChartOption;

        public bool IsTrendsSeriesVisible(string seriesTitle)
        {
            var trendsPageLayout = (Layout<View>)GetVisibleContentPage().Content;

            var trendsChart = trendsPageLayout.Children.OfType<SfChart>().First();

            return trendsChart.Series.First(x => x.Label.Equals(seriesTitle)).IsVisible;
        }

        RefreshView GetVisibleRefreshView()
        {
            var visibleContentPage = GetVisibleContentPage();

            if (visibleContentPage.Content is RefreshView refreshView)
                return refreshView;
            else if (visibleContentPage.Content is Layout<View> layout && layout.Children.OfType<RefreshView>().FirstOrDefault() is RefreshView layoutRefreshView)
                return layoutRefreshView;
            else
                throw new NotSupportedException($"{visibleContentPage.GetType()} Does Not Contain a RefreshView");
        }

        ContentPage GetVisibleContentPage()
        {
            return (ContentPage)Application.Current.MainPage.Navigation.ModalStack.LastOrDefault()
                    ?? (ContentPage)Application.Current.MainPage.Navigation.NavigationStack.Last();
        }
    }
}
#endif
