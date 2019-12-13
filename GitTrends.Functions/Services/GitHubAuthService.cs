using System;
using System.Net.Http;
using System.Threading.Tasks;
using GitTrends.Shared;

namespace GitTrends.Functions
{
    class GitHubAuthService : BaseApiService
    {
        readonly IGitHubAuthApi _gitHubAuthClient;

        public GitHubAuthService(IGitHubAuthApi gitHubAuthApi) => _gitHubAuthClient = gitHubAuthApi;

        public Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state) => AttemptAndRetry(() => _gitHubAuthClient.GetAccessToken(clientId, clientSecret, loginCode, state));
    }
}
