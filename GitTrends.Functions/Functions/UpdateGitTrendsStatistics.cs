using GitTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GitTrends.Functions;

class UpdateGitTrendsStatistics(GitHubApiV3Service gitHubApiV3Service, BlobStorageService blobStorageService)
{
	readonly GitHubApiV3Service _gitHubApiV3Service = gitHubApiV3Service;
	readonly BlobStorageService _blobStorageService = blobStorageService;

	[Function(nameof(UpdateGitTrendsStatistics))]
	public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, FunctionContext context)
	{
		var log = context.GetLogger<UpdateGitTrendsStatistics>();
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

		var getRepositoryTask = _gitHubApiV3Service.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, cancellationTokenSource.Token);
		var getGitTrendsContributorsTask = _gitHubApiV3Service.GetGitTrendsContributors(cancellationTokenSource.Token);

		await Task.WhenAll(getRepositoryTask, getGitTrendsContributorsTask).ConfigureAwait(false);

		var repository = await getRepositoryTask.ConfigureAwait(false);
		var contributors = await getGitTrendsContributorsTask.ConfigureAwait(false);

		var statistics = new GitTrendsStatisticsDTO(new Uri(repository.Url ?? throw new NullReferenceException()),
			repository.StarCount,
			repository.WatchersCount,
			repository.ForkCount,
			contributors);

		var blobName = $"Statistics_{DateTime.UtcNow:o}.json";
		log.LogInformation($"Saving Statistics to Blob Storage: {blobName}");

		await _blobStorageService.UploadStatistics(statistics, blobName).ConfigureAwait(false);
	}
}