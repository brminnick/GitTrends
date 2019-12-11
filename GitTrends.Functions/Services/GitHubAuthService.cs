using System;
using System.Net.Http;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends.Functions
{
    class GitHubAuthService : BaseApiService
    {
        readonly IGitHubAuthApi _gitHubAuthClient;

        public GitHubAuthService(GitHubAuthServiceClient client) => _gitHubAuthClient = client.Client;

        public Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state) => AttemptAndRetry(() => _gitHubAuthClient.GetAccessToken(clientId, clientSecret, loginCode, state));
    }

    class GitHubAuthServiceClient
    {
        public GitHubAuthServiceClient(HttpClient client)
        {
            client.BaseAddress = new Uri(GitHubConstants.GitHubAuthBaseUrl);
            client.DefaultRequestVersion = new Version(2, 0);

            Client = RestService.For<IGitHubAuthApi>(client);
        }

        public IGitHubAuthApi Client { get; }
    }
}
