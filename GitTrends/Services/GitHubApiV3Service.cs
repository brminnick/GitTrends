using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    abstract class GitHubApiV3Service : BaseMobileApiService
    {
        #region Constant Fields
        static readonly Lazy<IGitHubApiV3> _githubApiClient = new Lazy<IGitHubApiV3>(() => RestService.For<IGitHubApiV3>(CreateHttpClient(GitHubConstants.GitHubRestApiUrl)));
        #endregion

        #region Properties
        static IGitHubApiV3 GithubApiClient => _githubApiClient.Value;
        #endregion

        #region Methods
        public static async Task<RepositoryViewsModel> GetRepositoryViewStatistics(string owner, string repo)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            return await ExecuteMobilePollyFunction(() => GithubApiClient.GetRepositoryViewStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
        }

        public static async Task<RepositoryClonesModel> GetRepositoryCloneStatistics(string owner, string repo)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            return await ExecuteMobilePollyFunction(() => GithubApiClient.GetRepositoryCloneStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
        }
        #endregion
    }
}
