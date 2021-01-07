using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends.Functions
{
    class GitHubGraphQLApiService : BaseApiService
    {
        readonly IGitHubGraphQLApi _gitHubGraphQLClient;

        public GitHubGraphQLApiService(IGitHubGraphQLApi gitHubGraphQLApi) => _gitHubGraphQLClient = gitHubGraphQLApi;

        public Task<ApiResponse<GraphQLResponse<GitHubViewerResponse>>> ViewerLoginQuery(string token, CancellationToken cancellationToken) =>
            AttemptAndRetry(() => _gitHubGraphQLClient.ViewerLoginQuery(new ViewerLoginQueryContent(), $"Bearer {token}"), cancellationToken);
    }
}
