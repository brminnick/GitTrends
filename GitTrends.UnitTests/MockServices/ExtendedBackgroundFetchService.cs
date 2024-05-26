using GitTrends.Shared;
using Shiny.Jobs;

namespace GitTrends.UnitTests;

class ExtendedBackgroundFetchService : BackgroundFetchService
{
	readonly IJobManager _jobManager;

	public ExtendedBackgroundFetchService(IAppInfo appInfo,
		IJobManager jobManager,
		IAnalyticsService analyticsService,
		GitHubUserService gitHubUserService,
		GitHubApiV3Service gitHubApiV3Service,
		RepositoryDatabase repositoryDatabase,
		NotificationService notificationService,
		ReferringSitesDatabase referringSitesDatabase,
		GitHubGraphQLApiService gitHubGraphQLApiService,
		GitHubApiRepositoriesService gitHubApiRepositoriesService)
		: base(appInfo,
			jobManager,
			analyticsService,
			gitHubUserService,
			gitHubApiV3Service,
			repositoryDatabase,
			notificationService,
			referringSitesDatabase,
			gitHubGraphQLApiService,
			gitHubApiRepositoriesService)
	{
		_jobManager = jobManager;
	}

	public void CancelAllJobs()
	{
		_jobManager.CancelAll();
		QueuedJobsHash.Clear();
	}
}