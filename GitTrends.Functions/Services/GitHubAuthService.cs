using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends.Functions
{
    abstract class GitHubAuthService : BaseApiService
    {
        #region Constant Fields
        readonly static Lazy<IGitHubAuthApi> _githubAuthClient = new Lazy<IGitHubAuthApi>(() => RestService.For<IGitHubAuthApi>(CreateHttpClient(GitHubConstants.GitHubAuthBaseUrl)));
        #endregion

        #region Properties
        static IGitHubAuthApi GitHubAuthClient => _githubAuthClient.Value;
        #endregion

        #region Methods
        public static Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state) => ExecutePollyFunction(() => GitHubAuthClient.GetAccessToken(clientId, clientSecret, loginCode, state));
        #endregion
    }
}
