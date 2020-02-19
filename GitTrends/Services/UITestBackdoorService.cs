#if DEBUG
using System.Threading.Tasks;
using GitTrends.Shared;

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

            _gitHubAuthenticationService.Alias = alias;
            _gitHubAuthenticationService.AvatarUrl = avatarUri.ToString();
            _gitHubAuthenticationService.Name = name;
        }
    }
}
#endif
