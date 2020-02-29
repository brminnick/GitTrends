#if !AppStore
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class UITestBackdoorService
    {
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public UITestBackdoorService(GitHubAuthenticationService gitHubAuthenticationService, GitHubGraphQLApiService gitHubGraphQLApiService) =>
            (_gitHubAuthenticationService, _gitHubGraphQLApiService) = (gitHubAuthenticationService, gitHubGraphQLApiService);

        public async Task SetGitHubUser(string token)
        {
            await _gitHubAuthenticationService.SaveGitHubToken(new GitHubToken(token, string.Empty, "Bearer")).ConfigureAwait(false);

            var (alias, name, avatarUri) = await _gitHubGraphQLApiService.GetCurrentUserInfo().ConfigureAwait(false);

            GitHubAuthenticationService.Alias = alias;
            GitHubAuthenticationService.AvatarUrl = avatarUri.ToString();
            GitHubAuthenticationService.Name = name;
        }

        public Task TriggerPullToRefresh()
        {
            var contentPage = (ContentPage)Application.Current.MainPage.Navigation.ModalStack.FirstOrDefault()
                                ?? (ContentPage)Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();

            if (contentPage.Content is RefreshView refreshView)
                return triggerPullToRefresh(refreshView);
            else if (contentPage.Content is Layout<View> layout && layout.Children.OfType<RefreshView>().FirstOrDefault() is RefreshView layoutRefreshView)
                return triggerPullToRefresh(layoutRefreshView);
            else
                throw new NotSupportedException($"{contentPage.GetType()} Does Not Contain a RefreshView");

            static Task triggerPullToRefresh(RefreshView refreshView) => MainThread.InvokeOnMainThreadAsync(() => refreshView.IsRefreshing = true);
        }

        public IReadOnlyList<Repository> GetVisibleRepositoryList()
        {
            var repositoryPage = (RepositoryPage)Application.Current.MainPage.Navigation.NavigationStack.First();
            var repositoryViewModel = (RepositoryViewModel)repositoryPage.BindingContext;

            return repositoryViewModel.VisibleRepositoryList;
        }

        public IReadOnlyList<ReferringSiteModel> GetVisibleReferringSitesList()
        {
            var repositoryPage = (ReferringSitesPage)Application.Current.MainPage.Navigation.NavigationStack.First();
            var repositoryViewModel = (ReferringSitesViewModel)repositoryPage.BindingContext;

            return repositoryViewModel.ReferringSitesCollection;
        }
    }
}
#endif
