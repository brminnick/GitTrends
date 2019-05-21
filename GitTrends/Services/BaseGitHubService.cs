using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using Xamarin.Essentials;

namespace GitTrends
{
    public abstract class BaseGitHubService
    {
        const string _oauthTokenKey = "OAuthToken";

        static GitHubClient _githubClientHolder;

        protected static async Task<OauthToken> GetOAuthToken()
        {
            var serializedToken = await SecureStorage.GetAsync(_oauthTokenKey).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(serializedToken))
                return null;

            return await Task.Run(() => JsonConvert.DeserializeObject<OauthToken>(serializedToken)).ConfigureAwait(false);
        }

        protected static async Task SetOAuthToken(OauthToken token)
        {
            var serializedToken = await Task.Run(() => JsonConvert.SerializeObject(token)).ConfigureAwait(false);
            await SecureStorage.SetAsync(_oauthTokenKey, serializedToken).ConfigureAwait(false);

            var githubClient = await GetGitHubClient().ConfigureAwait(false);
            githubClient.Credentials = new Credentials(token.AccessToken);
        }

        protected static async ValueTask<GitHubClient> GetGitHubClient()
        {
            if (_githubClientHolder is null)
            {
                var token = await GetOAuthToken().ConfigureAwait(false);

                _githubClientHolder = new GitHubClient(new ProductHeaderValue(nameof(GitTrends)));

                if (token?.AccessToken != null)
                    _githubClientHolder.Credentials = new Credentials(token.AccessToken);
            }

            return _githubClientHolder;
        }
    }
}
