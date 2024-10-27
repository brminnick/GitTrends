using GitTrends.Common;
using Shiny.Jobs;

namespace GitTrends.UnitTests;

class ExtendedBackgroundFetchService(
	IJobManager jobManager,
	CleanDatabaseJob cleanDatabaseJob,
	RetryRepositoryStarsJob retryRepositoryStarsJob,
	RetryGetReferringSitesJob retryGetReferringSitesJob,
	NotifyTrendingRepositoriesJob notifyTrendingRepositoriesJob,
	RetryOrganizationsRepositoriesJob retryOrganizationsRepositoriesJob,
	RetryRepositoriesViewsClonesStarsJob retryRepositoriesViewsClonesStarsJob)
	: BackgroundFetchService(jobManager,
		cleanDatabaseJob,
		retryRepositoryStarsJob,
		retryGetReferringSitesJob,
		notifyTrendingRepositoriesJob,
		retryOrganizationsRepositoriesJob,
		retryRepositoriesViewsClonesStarsJob)
{
	readonly IJobManager _jobManager = jobManager;

	public void CancelAllJobs()
	{
		_jobManager.CancelAll();
		QueuedForegroundJobsHash.Clear();
	}
}