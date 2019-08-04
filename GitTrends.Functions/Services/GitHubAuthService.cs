using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends.Functions
{
    abstract class GitHubAuthService : BaseApiService
    {
        readonly static Lazy<IGitHubAuthApi> _githubAuthClient = new Lazy<IGitHubAuthApi>(() => RestService.For<IGitHubAuthApi>(CreateHttpClient(GitHubConstants.GitHubAuthBaseUrl)));

        static IGitHubAuthApi GitHubAuthClient => _githubAuthClient.Value;

        public static Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state) => ExecutePollyFunction(() => GitHubAuthClient.GetAccessToken(clientId, clientSecret, loginCode, state));
    }
}
