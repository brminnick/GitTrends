#if DEBUG
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

        public Task TriggerRepositoryPullToRefresh()
        {
            var repositoryPage = (RepositoryPage)Application.Current.MainPage.Navigation.NavigationStack.First();

            var refreshView = (RefreshView)repositoryPage.Content;
            return MainThread.InvokeOnMainThreadAsync(() => refreshView.IsRefreshing = true);
        }

        public IReadOnlyList<Repository> GetVisibleRepositoryList()
        {
            var repositoryPage = (RepositoryPage)Application.Current.MainPage.Navigation.NavigationStack.First();
            var repositoryViewModel = (RepositoryViewModel)repositoryPage.BindingContext;

            return repositoryViewModel.VisibleRepositoryList;
        }
    }
}
#endif
