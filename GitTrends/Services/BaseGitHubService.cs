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

        static readonly Lazy<GitHubClient> _githubClientHolder = new Lazy<GitHubClient>(() => new GitHubClient(new ProductHeaderValue(nameof(GitTrends))));

        protected static GitHubClient GitHubClient => _githubClientHolder.Value;

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

            GitHubClient.Credentials = new Credentials(token.AccessToken);
        }
    }
}
