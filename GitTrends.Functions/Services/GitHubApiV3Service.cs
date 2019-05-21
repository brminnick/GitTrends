using GitTrends.Shared;
using System;
using Refit;
using System.Threading.Tasks;

namespace GitTrends.Functions
{
    abstract class GitHubApiV3Service : BaseApiService
    {
        #region Constant Fields
        readonly static Lazy<IGitHubApiV3> _githubApiV3ClientHolder = new Lazy<IGitHubApiV3>(() => RestService.For<IGitHubApiV3>(CreateHttpClient(GitHubConstants.GitHubRestApiUrl)));
        #endregion

        #region Properties
        static IGitHubApiV3 GitHubApiV3Client => _githubApiV3ClientHolder.Value;
        #endregion

        #region Methods
        public static Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state) => ExecutePollyFunction(() => GitHubApiV3Client.GetAccessToken(clientId, clientSecret, loginCode, state));
        #endregion
    }
}
