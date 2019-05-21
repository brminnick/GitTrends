using System.Threading.Tasks;
using Octokit;
using Xamarin.Essentials;

namespace GitTrends
{
    public abstract class BaseGitHubService
    {
        const string _oauthTokenKey = "OAuthToken";

        static GitHubClient _githubClientHolder;

        protected static Task<string> GetAccessToken() => SecureStorage.GetAsync(_oauthTokenKey);

        protected static async Task SetAccessToken(string token)
        {
            await SecureStorage.SetAsync(_oauthTokenKey, token).ConfigureAwait(false);

            var githubClient = await GetGitHubClient().ConfigureAwait(false);
            githubClient.Credentials = new Credentials(token);
        }

        protected static async ValueTask<GitHubClient> GetGitHubClient()
        {
            if (_githubClientHolder is null)
            {
                var token = await GetAccessToken().ConfigureAwait(false);

                _githubClientHolder = new GitHubClient(new ProductHeaderValue(nameof(GitTrends)));

                if (!string.IsNullOrWhiteSpace(token))
                    _githubClientHolder.Credentials = new Credentials(token);
            }

            return _githubClientHolder;
        }
    }
}
