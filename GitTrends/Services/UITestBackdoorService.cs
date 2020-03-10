#if !AppStore
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
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

        RefreshView GetVisibleRefreshView()
        {
            var contentPage = (ContentPage)Application.Current.MainPage.Navigation.ModalStack.FirstOrDefault()
                                ?? (ContentPage)Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();

            if (contentPage.Content is RefreshView refreshView)
                return refreshView;
            else if (contentPage.Content is Layout<View> layout && layout.Children.OfType<RefreshView>().FirstOrDefault() is RefreshView layoutRefreshView)
                return layoutRefreshView;
            else
                throw new NotSupportedException($"{contentPage.GetType()} Does Not Contain a RefreshView");
        }
    }
}
#endif
