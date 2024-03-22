using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using Microsoft.Extensions.Logging;
using Refit;

namespace GitTrends.Functions;

class GitHubGraphQLApiService : BaseGitHubApiService
{
	readonly IGitHubGraphQLApi _gitHubGraphQLClient;

	public GitHubGraphQLApiService(IGitHubGraphQLApi gitHubGraphQLApi,
									ILogger<GitHubGraphQLApiService> logger,
									IGitHubApiStatusService gitHubApiStatusService) : base(gitHubApiStatusService, logger)
	{
		_gitHubGraphQLClient = gitHubGraphQLApi;
	}

	public Task<ApiResponse<GraphQLResponse<GitHubViewerLoginResponse>>> ViewerLoginQuery(string token, CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => _gitHubGraphQLClient.ViewerLoginQuery(new ViewerLoginQueryContent(), $"Bearer {token}"), cancellationToken);
}