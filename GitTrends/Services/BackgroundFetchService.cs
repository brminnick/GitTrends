using System.Text.Json;
using AsyncAwaitBestPractices;
using GitTrends.Common;
using Shiny.Jobs;

namespace GitTrends;

public class BackgroundFetchService
{
	readonly IJobManager _jobManager;
	readonly CleanDatabaseJob _cleanDatabaseJob;
	readonly RetryRepositoryStarsJob _retryRepositoryStarsJob;
	readonly RetryGetReferringSitesJob _retryGetReferringSitesJob;
	readonly NotifyTrendingRepositoriesJob _notifyTrendingRepositoriesJob;
	readonly RetryOrganizationsRepositoriesJob _retryOrganizationsRepositoriesJob;
	readonly RetryRepositoriesViewsClonesStarsJob _retryRepositoriesViewsClonesStarsJob;


	public BackgroundFetchService(
		IJobManager jobManager,
		CleanDatabaseJob cleanDatabaseJob,
		RetryRepositoryStarsJob retryRepositoryStarsJob,
		RetryGetReferringSitesJob retryGetReferringSitesJob,
		NotifyTrendingRepositoriesJob notifyTrendingRepositoriesJob,
		RetryOrganizationsRepositoriesJob retryOrganizationsRepositoriesJob,
		RetryRepositoriesViewsClonesStarsJob retryRepositoriesViewsClonesStarsJob)
	{
		_jobManager = jobManager;
		_cleanDatabaseJob = cleanDatabaseJob;
		_retryRepositoryStarsJob = retryRepositoryStarsJob;
		_retryGetReferringSitesJob = retryGetReferringSitesJob;
		_notifyTrendingRepositoriesJob = notifyTrendingRepositoriesJob;
		_retryOrganizationsRepositoriesJob = retryOrganizationsRepositoriesJob;
		_retryRepositoriesViewsClonesStarsJob = retryRepositoriesViewsClonesStarsJob;

		GitHubApiRepositoriesService.AbuseRateLimitFound_GetReferringSites += HandleAbuseRateLimitFound_GetReferringSites;
		GitHubGraphQLApiService.AbuseRateLimitFound_GetOrganizationRepositories += HandleAbuseRateLimitFound_GetOrganizationRepositories;
		GitHubApiRepositoriesService.AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData += HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData;
	}

	public IReadOnlyList<string> QueuedForegroundJobsList => [.. QueuedForegroundJobsHash];
	protected HashSet<string> QueuedForegroundJobsHash { get; } = [];

	public void Initialize()
	{
		//Required to ensure the service is instantiated in the Dependency Injection Container and events in the constructor are subscribed
		var temp = QueuedForegroundJobsList;
	}

	public bool IsFetchingViewsClonesStarsInBackground(Repository repository) =>
		QueuedForegroundJobsList.Any(x => x == _retryRepositoriesViewsClonesStarsJob.GetJobIdentifier(repository));

	public bool IsFetchingStarsInBackground(Repository repository) =>
		QueuedForegroundJobsList.Any(x => x == _retryRepositoryStarsJob.GetJobIdentifier(repository));

	public bool TryScheduleRetryOrganizationsRepositories(string organizationName, TimeSpan? delay = null)
	{
		lock (_retryOrganizationsRepositoriesJob.Identifier)
		{
			if (QueuedForegroundJobsList.Contains(_retryOrganizationsRepositoriesJob.GetJobIdentifier(organizationName)))
				return false;

			ScheduleRetryOrganizationsRepositories(organizationName, delay).SafeFireAndForget();
			return true;
		}
	}

	public bool TryScheduleRetryRepositoriesStars(Repository repository, TimeSpan? delay = null)
	{
		lock (_retryRepositoryStarsJob.Identifier)
		{
			if (QueuedForegroundJobsList.Contains(_retryRepositoryStarsJob.GetJobIdentifier(repository)))
				return false;

			ScheduleRetryRepositoriesStars(repository, delay).SafeFireAndForget();
			return true;
		}
	}

	public bool TryScheduleRetryRepositoriesViewsClonesStars(Repository repository, TimeSpan? delay = null)
	{
		lock (_retryRepositoriesViewsClonesStarsJob.Identifier)
		{
			if (QueuedForegroundJobsList.Contains(_retryRepositoriesViewsClonesStarsJob.GetJobIdentifier(repository)))
				return false;

			ScheduleRetryRepositoriesViewsClonesStars(repository, delay).SafeFireAndForget();
			return true;
		}
	}

	public bool TryScheduleRetryGetReferringSites(Repository repository, TimeSpan? delay = null)
	{
		lock (_retryGetReferringSitesJob.Identifier)
		{
			if (QueuedForegroundJobsList.Contains(_retryGetReferringSitesJob.GetJobIdentifier(repository)))
				return false;

			ScheduleRetryGetReferringSites(repository, delay).SafeFireAndForget();
			return true;
		}
	}

	public bool TryScheduleCleanUpDatabase()
	{
		lock (_cleanDatabaseJob.Identifier)
		{
			if (QueuedForegroundJobsList.Contains(_cleanDatabaseJob.Identifier))
				return false;

			ScheduleCleanUpDatabase().SafeFireAndForget();
			return true;
		}
	}

	public bool TryScheduleNotifyTrendingRepositories()
	{
		lock (_notifyTrendingRepositoriesJob.NotifyTrendingRepositoriesIdentifier)
		{
			if (QueuedForegroundJobsList.Contains(_notifyTrendingRepositoriesJob.NotifyTrendingRepositoriesIdentifier))
				return false;

			ScheduleNotifyTrendingRepositories().SafeFireAndForget();
			return true;
		}
	}

	async Task ScheduleRetryOrganizationsRepositories(string organizationName, TimeSpan? delay)
	{
		var identifier = _retryOrganizationsRepositoriesJob.GetJobIdentifier(organizationName);
		QueuedForegroundJobsHash.Add(identifier);

		var retryOrganizationsRepositoriesJobCompletedTCS = new TaskCompletionSource();

		_jobManager.RunTask(identifier, async cancellationToken =>
		{
			if (delay is not null)
				await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);

			await _retryOrganizationsRepositoriesJob.Run(_retryOrganizationsRepositoriesJob.GetJobInfo(organizationName, true), cancellationToken);
			retryOrganizationsRepositoriesJobCompletedTCS.SetResult();
		});

		await retryOrganizationsRepositoriesJobCompletedTCS.Task.ConfigureAwait(false);

		QueuedForegroundJobsHash.Remove(identifier);
	}

	async Task ScheduleRetryRepositoriesViewsClonesStars(Repository repository, TimeSpan? delay = null)
	{
		var identifier = _retryRepositoriesViewsClonesStarsJob.GetJobIdentifier(repository);
		QueuedForegroundJobsHash.Add(identifier);

		var retryRepositoriesViewsClonesStarsTaskTCS = new TaskCompletionSource();

		_jobManager.RunTask(identifier, async cancellationToken =>
		{
			if (delay is not null)
				await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);

			await _retryRepositoriesViewsClonesStarsJob.Run(_retryRepositoriesViewsClonesStarsJob.GetJobInfo(repository, true), cancellationToken);

			retryRepositoriesViewsClonesStarsTaskTCS.SetResult();
		});

		await retryRepositoriesViewsClonesStarsTaskTCS.Task.ConfigureAwait(false);

		QueuedForegroundJobsHash.Remove(_retryRepositoriesViewsClonesStarsJob.GetJobIdentifier(repository));
	}

	async Task ScheduleRetryRepositoriesStars(Repository repository, TimeSpan? delay = null)
	{
		QueuedForegroundJobsHash.Add(_retryRepositoryStarsJob.GetJobIdentifier(repository));

		var retryRepositoriesStarsTCS = new TaskCompletionSource();

		_jobManager.RunTask(_retryRepositoryStarsJob.GetJobIdentifier(repository), async cancellationToken =>
		{
			if (delay is not null)
				await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);

			await _retryRepositoryStarsJob.Run(_retryRepositoryStarsJob.GetJobInfo(repository, true), cancellationToken).ConfigureAwait(false);
			retryRepositoriesStarsTCS.SetResult();
		});

		await retryRepositoriesStarsTCS.Task.ConfigureAwait(false);

		QueuedForegroundJobsHash.Remove(_retryRepositoryStarsJob.GetJobIdentifier(repository));
	}

	async Task ScheduleRetryGetReferringSites(Repository repository, TimeSpan? delay = null)
	{
		QueuedForegroundJobsHash.Add(_retryGetReferringSitesJob.GetJobIdentifier(repository));

		var retryGetReferringSitesTCS = new TaskCompletionSource();

		_jobManager.RunTask(_retryGetReferringSitesJob.GetJobIdentifier(repository), async cancellationToken =>
		{
			if (delay is not null)
				await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);

			await _retryGetReferringSitesJob.Run(_retryGetReferringSitesJob.GetJobInfo(repository, true), cancellationToken).ConfigureAwait(false);
			retryGetReferringSitesTCS.SetResult();
		});

		await retryGetReferringSitesTCS.Task.ConfigureAwait(false);

		QueuedForegroundJobsHash.Remove(_retryGetReferringSitesJob.GetJobIdentifier(repository));
	}

	async Task ScheduleCleanUpDatabase()
	{
		QueuedForegroundJobsHash.Add(_cleanDatabaseJob.Identifier);

		var databaseCleanupTCS = new TaskCompletionSource();

		_jobManager.RunTask(_cleanDatabaseJob.Identifier, async cancellationToken =>
		{
			await _cleanDatabaseJob.Run(_cleanDatabaseJob.GetJobInfo(true), cancellationToken);
			databaseCleanupTCS.SetResult();
		});

		await databaseCleanupTCS.Task.ConfigureAwait(false);

		QueuedForegroundJobsHash.Remove(_cleanDatabaseJob.Identifier);
	}

	async Task ScheduleNotifyTrendingRepositories()
	{
		QueuedForegroundJobsHash.Add(_notifyTrendingRepositoriesJob.NotifyTrendingRepositoriesIdentifier);

		var notifyTrendingRepositoriesTCS = new TaskCompletionSource();

		_jobManager.RunTask(_notifyTrendingRepositoriesJob.NotifyTrendingRepositoriesIdentifier, async cancellationToken =>
		{
			await _notifyTrendingRepositoriesJob.Run(_notifyTrendingRepositoriesJob.GetJobInfo(true), cancellationToken).ConfigureAwait(false);
			notifyTrendingRepositoriesTCS.SetResult();
		});

		await notifyTrendingRepositoriesTCS.Task.ConfigureAwait(false);

		QueuedForegroundJobsHash.Remove(_notifyTrendingRepositoriesJob.NotifyTrendingRepositoriesIdentifier);
	}

	void HandleAbuseRateLimitFound_GetOrganizationRepositories(object? sender, (string OrganizationName, TimeSpan RetryTimeSpan) data) =>
		TryScheduleRetryOrganizationsRepositories(data.OrganizationName, data.RetryTimeSpan);

	void HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(object? sender, (Repository Repository, TimeSpan RetryTimeSpan) data) =>
		TryScheduleRetryRepositoriesViewsClonesStars(data.Repository, data.RetryTimeSpan);

	void HandleAbuseRateLimitFound_GetReferringSites(object? sender, (Repository Repository, TimeSpan RetryTimeSpan) data) =>
		TryScheduleRetryGetReferringSites(data.Repository, data.RetryTimeSpan);
}