using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Shiny.Jobs;

namespace GitTrends;

public class BackgroundFetchService
{
	static readonly AsyncAwaitBestPractices.WeakEventManager _eventManager = new();
	static readonly WeakEventManager<bool> _scheduleNotifyTrendingRepositoriesCompletedEventManager = new();
	static readonly WeakEventManager<string> _scheduleRetryOrganizationsRepositoriesCompletedEventManager = new();
	static readonly WeakEventManager<Repository> _repositoryEventManager = new();
	static readonly WeakEventManager<MobileReferringSiteModel> _mobileReferringSiteRetrievedEventManager = new();

	readonly IJobManager _jobManager;
	readonly IAnalyticsService _analyticsService;
	readonly GitHubUserService _gitHubUserService;
	readonly GitHubApiV3Service _gitHubApiV3Service;
	readonly RepositoryDatabase _repositoryDatabase;
	readonly NotificationService _notificationService;
	readonly ReferringSitesDatabase _referringSitesDatabase;
	readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
	readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService;


	public BackgroundFetchService(IAppInfo appInfo,
									IJobManager jobManager,
									IAnalyticsService analyticsService,
									GitHubUserService gitHubUserService,
									GitHubApiV3Service gitHubApiV3Service,
									RepositoryDatabase repositoryDatabase,
									NotificationService notificationService,
									ReferringSitesDatabase referringSitesDatabase,
									GitHubGraphQLApiService gitHubGraphQLApiService,
									GitHubApiRepositoriesService gitHubApiRepositoriesService)
	{
		_jobManager = jobManager;
		_analyticsService = analyticsService;
		_gitHubUserService = gitHubUserService;
		_gitHubApiV3Service = gitHubApiV3Service;
		_repositoryDatabase = repositoryDatabase;
		_notificationService = notificationService;
		_referringSitesDatabase = referringSitesDatabase;
		_gitHubGraphQLApiService = gitHubGraphQLApiService;
		_gitHubApiRepositoriesService = gitHubApiRepositoriesService;

		CleanUpDatabaseIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleCleanUpDatabase)}";
		RetryGetReferringSitesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryGetReferringSites)}";
		RetryRepositoriesStarsIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryRepositoriesStars)}";
		NotifyTrendingRepositoriesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleNotifyTrendingRepositories)}";
		RetryOrganizationsReopsitoriesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryOrganizationsRepositories)}";
		RetryRepositoriesViewsClonesStarsIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryRepositoriesViewsClonesStars)}";

		GitHubApiRepositoriesService.AbuseRateLimitFound_GetReferringSites += HandleAbuseRateLimitFound_GetReferringSites;
		GitHubGraphQLApiService.AbuseRateLimitFound_GetOrganizationRepositories += HandleAbuseRateLimitFound_GetOrganizationRepositories;
		GitHubApiRepositoriesService.AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData += HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData;
	}

	public static event EventHandler DatabaseCleanupCompleted
	{
		add => _eventManager.AddEventHandler(value);
		remove => _eventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<MobileReferringSiteModel> MobileReferringSiteRetrieved
	{
		add => _mobileReferringSiteRetrievedEventManager.AddEventHandler(value);
		remove => _mobileReferringSiteRetrievedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<bool> ScheduleNotifyTrendingRepositoriesCompleted
	{
		add => _scheduleNotifyTrendingRepositoriesCompletedEventManager.AddEventHandler(value);
		remove => _scheduleNotifyTrendingRepositoriesCompletedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<string> ScheduleRetryOrganizationsRepositoriesCompleted
	{
		add => _scheduleRetryOrganizationsRepositoriesCompletedEventManager.AddEventHandler(value);
		remove => _scheduleRetryOrganizationsRepositoriesCompletedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<Repository> ScheduleRetryRepositoriesViewsClonesStarsCompleted
	{
		add => _repositoryEventManager.AddEventHandler(value);
		remove => _repositoryEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<Repository> ScheduleRetryRepositoriesStarsCompleted
	{
		add => _repositoryEventManager.AddEventHandler(value);
		remove => _repositoryEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<Repository> ScheduleRetryGetReferringSitesCompleted
	{
		add => _repositoryEventManager.AddEventHandler(value);
		remove => _repositoryEventManager.RemoveEventHandler(value);
	}

	public IReadOnlyList<string> QueuedJobs => [.. QueuedJobsHash];

	protected HashSet<string> QueuedJobsHash { get; } = [];

	public string CleanUpDatabaseIdentifier { get; }
	public string RetryRepositoriesStarsIdentifier { get; }
	public string RetryGetReferringSitesIdentifier { get; }
	public string NotifyTrendingRepositoriesIdentifier { get; }
	public string RetryOrganizationsReopsitoriesIdentifier { get; }
	public string RetryRepositoriesViewsClonesStarsIdentifier { get; }

	public void Initialize()
	{
		//Required to ensure the service is instantiated in the Dependency Injection Container and events in the constructor are subscribed
		var temp = QueuedJobs;
	}

	public bool TryScheduleRetryOrganizationsRepositories(string organizationName, TimeSpan? delay = null)
	{
		lock (RetryOrganizationsReopsitoriesIdentifier)
		{
			if (QueuedJobs.Contains(GetRetryOrganizationsRepositoriesIdentifier(organizationName)))
				return false;

			ScheduleRetryOrganizationsRepositories(organizationName, delay);
			return true;
		}
	}

	public bool TryScheduleRetryRepositoriesStars(Repository repository, TimeSpan? delay = null)
	{
		lock (RetryRepositoriesViewsClonesStarsIdentifier)
		{
			if (QueuedJobs.Contains(GetRetryRepositoriesStarsIdentifier(repository)))
				return false;

			ScheduleRetryRepositoriesStars(repository, delay);
			return true;
		}
	}

	public bool TryScheduleRetryRepositoriesViewsClonesStars(Repository repository, TimeSpan? delay = null)
	{
		lock (RetryRepositoriesViewsClonesStarsIdentifier)
		{
			if (QueuedJobs.Contains(GetRetryRepositoriesViewsClonesStarsIdentifier(repository)))
				return false;

			ScheduleRetryRepositoriesViewsClonesStars(repository, delay);
			return true;
		}
	}

	public bool TryScheduleRetryGetReferringSites(Repository repository, TimeSpan? delay = null)
	{
		lock (RetryGetReferringSitesIdentifier)
		{
			if (QueuedJobs.Contains(GetRetryGetReferringSitesIdentifier(repository)))
				return false;

			ScheduleRetryGetReferringSites(repository, delay);
			return true;
		}
	}

	public bool TryScheduleCleanUpDatabase()
	{
		lock (CleanUpDatabaseIdentifier)
		{
			if (QueuedJobs.Contains(CleanUpDatabaseIdentifier))
				return false;

			ScheduleCleanUpDatabase();
			return true;
		}
	}

	public bool TryScheduleNotifyTrendingRepositories(CancellationToken cancellationToken)
	{
		lock (NotifyTrendingRepositoriesIdentifier)
		{
			if (QueuedJobs.Contains(NotifyTrendingRepositoriesIdentifier))
				return false;

			ScheduleNotifyTrendingRepositories(cancellationToken);
			return true;
		}
	}

	public string GetRetryGetReferringSitesIdentifier(Repository repository) => $"{RetryGetReferringSitesIdentifier}.{repository.Url}";
	public string GetRetryRepositoriesStarsIdentifier(Repository repository) => $"{RetryRepositoriesStarsIdentifier}.{repository.Url}";
	public string GetRetryRepositoriesViewsClonesStarsIdentifier(Repository repository) => $"{RetryRepositoriesViewsClonesStarsIdentifier}.{repository.Url}";
	public string GetRetryOrganizationsRepositoriesIdentifier(string organizationName) => $"{RetryOrganizationsReopsitoriesIdentifier}.{organizationName}";

	void ScheduleRetryOrganizationsRepositories(string organizationName, TimeSpan? delay)
	{
		QueuedJobsHash.Add(GetRetryOrganizationsRepositoriesIdentifier(organizationName));

		_jobManager.RunTask(GetRetryOrganizationsRepositoriesIdentifier(organizationName), async cancellationToken =>
		{
			_analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryOrganizationsRepositories)} Triggered", nameof(delay), delay?.ToString() ?? "null");

			if (delay is not null)
				await Task.Delay(delay.Value, CancellationToken.None).ConfigureAwait(false);

			try
			{
				var repositories = await _gitHubGraphQLApiService.GetOrganizationRepositories(organizationName, cancellationToken).ConfigureAwait(false);

				foreach (var repository in repositories)
					ScheduleRetryRepositoriesViewsClonesStars(repository);

			}
			catch (Exception e)
			{
				_analyticsService.Report(e);
			}
			finally
			{
				OnScheduleRetryOrganizationsRepositoriesCompleted(organizationName);
			}
		});
	}

	void ScheduleRetryRepositoriesViewsClonesStars(Repository repository, TimeSpan? delay = null)
	{
		QueuedJobsHash.Add(GetRetryRepositoriesViewsClonesStarsIdentifier(repository));

		_jobManager.RunTask(GetRetryRepositoriesViewsClonesStarsIdentifier(repository), async cancellationToken =>
		{
			_analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryRepositoriesViewsClonesStars)} Triggered", nameof(delay), delay?.ToString() ?? "null");

			if (delay is not null)
				await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);

			using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryRepositoriesViewsClonesStars)} Executed");

			try
			{
				await foreach (var repositoryWithViewsClonesData in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(new List<Repository> { repository }, cancellationToken).ConfigureAwait(false))
				{
					repository = repositoryWithViewsClonesData;
				}

				await foreach (var repositoryWithViewsClonesStarsData in _gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(new List<Repository> { repository }, cancellationToken).ConfigureAwait(false))
				{
					await _repositoryDatabase.SaveRepository(repositoryWithViewsClonesStarsData, cancellationToken).ConfigureAwait(false);
					OnScheduleRetryRepositoriesViewsClonesStarsCompleted(repositoryWithViewsClonesStarsData);
				}

			}
			catch (Exception e)
			{
				_analyticsService.Report(e);
			}
		});
	}

	void ScheduleRetryRepositoriesStars(Repository repository, TimeSpan? delay = null)
	{
		QueuedJobsHash.Add(GetRetryRepositoriesStarsIdentifier(repository));

		_jobManager.RunTask(GetRetryRepositoriesStarsIdentifier(repository), async cancellationToken =>
		{
			_analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryRepositoriesStars)} Triggered", nameof(delay), delay?.ToString() ?? "null");

			if (delay is not null)
				await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);

			using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryRepositoriesStars)} Executed");

			try
			{
				await foreach (var repositoryWithViewsClonesStarsData in _gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(new List<Repository> { repository }, cancellationToken).ConfigureAwait(false))
				{
					await _repositoryDatabase.SaveRepository(repositoryWithViewsClonesStarsData, cancellationToken).ConfigureAwait(false);
					OnScheduleRetryRepositoriesStarsCompleted(repositoryWithViewsClonesStarsData);
				}
			}
			catch (Exception e)
			{
				_analyticsService.Report(e);
			}
		});
	}

	void ScheduleRetryGetReferringSites(Repository repository, TimeSpan? delay = null)
	{
		QueuedJobsHash.Add(GetRetryGetReferringSitesIdentifier(repository));

		_jobManager.RunTask(GetRetryGetReferringSitesIdentifier(repository), async cancellationToken =>
		{
			_analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryGetReferringSites)} Triggered", nameof(delay), delay?.ToString() ?? "null");

			if (delay is not null)
				await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);

			try
			{
				var referringSites = await _gitHubApiRepositoriesService.GetReferringSites(repository, cancellationToken).ConfigureAwait(false);

				await foreach (var mobileReferringSite in _gitHubApiRepositoriesService.GetMobileReferringSites(referringSites, repository.Url, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token).ConfigureAwait(false))
				{
					await _referringSitesDatabase.SaveReferringSite(mobileReferringSite, repository.Url, cancellationToken).ConfigureAwait(false);
					OnMobileReferringSiteRetrieved(mobileReferringSite);
				}
			}
			catch (Exception e)
			{
				_analyticsService.Report(e);
			}
			finally
			{
				OnScheduleRetryGetReferringSitesCompleted(repository);
			}
		});
	}

	void ScheduleCleanUpDatabase()
	{
		QueuedJobsHash.Add(CleanUpDatabaseIdentifier);

		_jobManager.RunTask(CleanUpDatabaseIdentifier, async cancellationToken =>
		{
			using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleCleanUpDatabase)} Triggered");

			try
			{
				await Task.WhenAll(_referringSitesDatabase.DeleteExpiredData(cancellationToken), _repositoryDatabase.DeleteExpiredData(cancellationToken)).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_analyticsService.Report(e);
			}
			finally
			{
				OnDatabaseCleanupCompleted();
			}
		});
	}

	void ScheduleNotifyTrendingRepositories(CancellationToken cancellationToken)
	{
		QueuedJobsHash.Add(NotifyTrendingRepositoriesIdentifier);

		_jobManager.RunTask(NotifyTrendingRepositoriesIdentifier, async cancellationToken =>
		{
			try
			{
				using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleNotifyTrendingRepositories)} Triggered");

				if (!_gitHubUserService.IsAuthenticated || _gitHubUserService.IsDemoUser)
				{
					OnScheduleNotifyTrendingRepositoriesCompleted(false);
				}
				else
				{
					var trendingRepositories = await GetTrendingRepositories(cancellationToken).ConfigureAwait(false);
					await _notificationService.TrySendTrendingNotificaiton(trendingRepositories).ConfigureAwait(false);

					OnScheduleNotifyTrendingRepositoriesCompleted(true);
				}
			}
			catch (Exception e)
			{
				_analyticsService.Report(e);
				OnScheduleNotifyTrendingRepositoriesCompleted(false);
			}
		});
	}

	async Task<IReadOnlyList<Repository>> GetTrendingRepositories(CancellationToken cancellationToken)
	{
		if (!_gitHubUserService.IsDemoUser && !string.IsNullOrEmpty(_gitHubUserService.Alias))
		{
			var repositoriesFromDatabase = await _repositoryDatabase.GetRepositories(cancellationToken).ConfigureAwait(false);
			IReadOnlyList<string> favoriteRepositoryUrls = repositoriesFromDatabase.Where(static x => x.IsFavorite is true).Select(static x => x.Url).ToList();

			var retrievedRepositoryList = new List<Repository>();
			await foreach (var repository in _gitHubGraphQLApiService.GetRepositories(_gitHubUserService.Alias, cancellationToken).ConfigureAwait(false))
			{
				if (favoriteRepositoryUrls.Contains(repository.Url))
					retrievedRepositoryList.Add(repository with { IsFavorite = true });
				else
					retrievedRepositoryList.Add(repository);
			}

			var retrievedRepositoryList_NoDuplicatesNoForks = retrievedRepositoryList.RemoveForksDuplicatesAndArchives(static x => x.ContainsViewsClonesData);

			IReadOnlyList<Repository> repositoriesToUpdate = repositoriesFromDatabase.Where(x => _gitHubUserService.ShouldIncludeOrganizations || x.OwnerLogin == _gitHubUserService.Alias) // Only include organization repositories if `ShouldIncludeOrganizations` is true
										.Where(static x => x.DataDownloadedAt < DateTimeOffset.Now.Subtract(TimeSpan.FromHours(12))) // Cached repositories that haven't been updated in 12 hours 
										.Concat(retrievedRepositoryList_NoDuplicatesNoForks) // Add downloaded repositories
										.GroupBy(static x => x.Name).Select(static x => x.FirstOrDefault(static x => x.ContainsViewsClonesStarsData) ?? x.First()).ToList(); // Remove duplicate repositories


			var retrievedRepositoriesWithViewsAndClonesData = new List<Repository>();
			await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(repositoriesToUpdate, cancellationToken).ConfigureAwait(false))
			{
				retrievedRepositoriesWithViewsAndClonesData.Add(retrievedRepositoryWithViewsAndClonesData);
			}

			var trendingRepositories = new List<Repository>();
			await foreach (var retrievedRepositoryWithStarsData in _gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(retrievedRepositoriesWithViewsAndClonesData, cancellationToken))
			{
				try
				{
					await _repositoryDatabase.SaveRepository(retrievedRepositoryWithStarsData, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					_analyticsService.Report(e);
				}

				if (retrievedRepositoryWithStarsData.IsTrending)
					trendingRepositories.Add(retrievedRepositoryWithStarsData);
			}

			return trendingRepositories;
		}

		return [];
	}

	void HandleAbuseRateLimitFound_GetOrganizationRepositories(object? sender, (string OrganizationName, TimeSpan RetryTimeSpan) data) =>
		TryScheduleRetryOrganizationsRepositories(data.OrganizationName, data.RetryTimeSpan);

	void HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(object? sender, (Repository Repository, TimeSpan RetryTimeSpan) data) =>
		TryScheduleRetryRepositoriesViewsClonesStars(data.Repository, data.RetryTimeSpan);

	void HandleAbuseRateLimitFound_GetReferringSites(object? sender, (Repository Repository, TimeSpan RetryTimeSpan) data) =>
		TryScheduleRetryGetReferringSites(data.Repository, data.RetryTimeSpan);

	void OnScheduleRetryRepositoriesViewsClonesStarsCompleted(in Repository repository)
	{
		QueuedJobsHash.Remove(GetRetryRepositoriesViewsClonesStarsIdentifier(repository));
		_repositoryEventManager.RaiseEvent(this, repository, nameof(ScheduleRetryRepositoriesViewsClonesStarsCompleted));
	}

	void OnScheduleRetryRepositoriesStarsCompleted(in Repository repository)
	{
		QueuedJobsHash.Remove(GetRetryRepositoriesStarsIdentifier(repository));
		_repositoryEventManager.RaiseEvent(this, repository, nameof(ScheduleRetryRepositoriesStarsCompleted));
	}

	void OnDatabaseCleanupCompleted()
	{
		QueuedJobsHash.Remove(CleanUpDatabaseIdentifier);
		_eventManager.RaiseEvent(this, EventArgs.Empty, nameof(DatabaseCleanupCompleted));
	}

	void OnScheduleNotifyTrendingRepositoriesCompleted(in bool result)
	{
		QueuedJobsHash.Remove(NotifyTrendingRepositoriesIdentifier);
		_scheduleNotifyTrendingRepositoriesCompletedEventManager.RaiseEvent(this, result, nameof(ScheduleNotifyTrendingRepositoriesCompleted));
	}

	void OnScheduleRetryOrganizationsRepositoriesCompleted(in string organizationName)
	{
		QueuedJobsHash.Remove(GetRetryOrganizationsRepositoriesIdentifier(organizationName));
		_scheduleRetryOrganizationsRepositoriesCompletedEventManager.RaiseEvent(this, organizationName, nameof(ScheduleRetryOrganizationsRepositoriesCompleted));
	}

	void OnScheduleRetryGetReferringSitesCompleted(in Repository repository)
	{
		QueuedJobsHash.Remove(GetRetryGetReferringSitesIdentifier(repository));
		_repositoryEventManager.RaiseEvent(this, repository, nameof(ScheduleRetryGetReferringSitesCompleted));
	}

	void OnMobileReferringSiteRetrieved(in MobileReferringSiteModel referringSite) =>
		_mobileReferringSiteRetrievedEventManager.RaiseEvent(this, referringSite, nameof(MobileReferringSiteRetrieved));
}