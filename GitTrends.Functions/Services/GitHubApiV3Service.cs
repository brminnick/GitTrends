using GitHubApiStatus;
using GitTrends.Common;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions;

class GitHubApiV3Service(
	IGitHubApiV3 gitHubApiV3Client,
	ILogger<GitHubApiV3Service> logger,
	IGitHubApiStatusService gitHubApiStatusService) : BaseGitHubApiService(gitHubApiStatusService, logger)
{

	public Task<RepositoryFile> GetGitTrendsFile(string fileName, CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => gitHubApiV3Client.GetGitTrendsFile(fileName, cancellationToken));

	public Task<IReadOnlyList<Contributor>> GetGitTrendsContributors(CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => gitHubApiV3Client.GetContributors(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, cancellationToken));

	public Task<GetRepositoryResponse> GetRepository(string owner, string repositoryName, CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => gitHubApiV3Client.GetRepository(owner, repositoryName, cancellationToken));
}