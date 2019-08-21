using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public class GitHubApiV3Service : BaseMobileApiService
    {
        readonly Lazy<IGitHubApiV3> _githubApiClient = new Lazy<IGitHubApiV3>(() => RestService.For<IGitHubApiV3>(CreateHttpClient(GitHubConstants.GitHubRestApiUrl)));

        IGitHubApiV3 GithubApiClient => _githubApiClient.Value;

        public async Task<RepositoryViewsModel> GetRepositoryViewStatistics(string owner, string repo)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            return await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryViewStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
        }

        public async Task<RepositoryClonesModel> GetRepositoryCloneStatistics(string owner, string repo)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            return await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryCloneStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
        }
    }
}
