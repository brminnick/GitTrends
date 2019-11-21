using System;
using System.Net.Http;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends.Functions
{
    abstract class GitHubAuthService : BaseApiService
    {
        readonly static Lazy<IGitHubAuthApi> _githubAuthClient = new Lazy<IGitHubAuthApi>(() => RestService.For<IGitHubAuthApi>(CreateFunctionsHttpClient(GitHubConstants.GitHubAuthBaseUrl)));

        static IGitHubAuthApi GitHubAuthClient => _githubAuthClient.Value;

        public static Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state) => AttemptAndRetry(() => GitHubAuthClient.GetAccessToken(clientId, clientSecret, loginCode, state));

        static HttpClient CreateFunctionsHttpClient(string url)
        {
            var client = CreateHttpClient(url);
            client.DefaultRequestVersion = new Version(2, 0);

            return client;
        }
    }
}
