using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitTrends.Shared;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions;

class GitHubApiV3Service : BaseGitHubApiService
{
	readonly IGitHubApiV3 _gitHubApiV3Client;

	public GitHubApiV3Service(IGitHubApiV3 gitHubApiV3Client,
								ILogger<GitHubApiV3Service> logger,
								IGitHubApiStatusService gitHubApiStatusService) : base(gitHubApiStatusService, logger)
	{
		_gitHubApiV3Client = gitHubApiV3Client;
	}

	public Task<RepositoryFile> GetGitTrendsFile(string fileName, CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => _gitHubApiV3Client.GetGitTrendsFile(fileName), cancellationToken);

	public Task<IReadOnlyList<Contributor>> GetGitTrendsContributors(CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => _gitHubApiV3Client.GetContributors(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName), cancellationToken);

	public Task<GetRepositoryResponse> GetRepository(string owner, string repositoryName, CancellationToken cancellationToken) =>
		AttemptAndRetry_Functions(() => _gitHubApiV3Client.GetRepository(owner, repositoryName), cancellationToken);
}