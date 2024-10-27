using GitHubApiStatus;
using GitTrends.Common;
using Microsoft.Extensions.Logging;
using Refit;

namespace GitTrends.Functions;

class GitHubGraphQLApiService(
	IGitHubGraphQLApi gitHubGraphQLApi,
	ILogger<GitHubGraphQLApiService> logger,
	IGitHubApiStatusService gitHubApiStatusService) : BaseGitHubApiService(gitHubApiStatusService, logger)
{
	public Task<ApiResponse<GraphQLResponse<GitHubViewerLoginResponse>>> ViewerLoginQuery(string token, CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => gitHubGraphQLApi.ViewerLoginQuery(new ViewerLoginQueryContent(), $"Bearer {token}", cancellationToken));
}