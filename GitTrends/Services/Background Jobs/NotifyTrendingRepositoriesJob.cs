using AsyncAwaitBestPractices;
using GitTrends.Common;
using Shiny.Jobs;

namespace GitTrends;

public class NotifyTrendingRepositoriesJob(
	IAppInfo appInfo,
	IAnalyticsService analyticsService,
	GitHubUserService gitHubUserService,
	RepositoryDatabase repositoryDatabase,
	NotificationService notificationService,
	GitHubGraphQLApiService gitHubGraphQlApiService,
	GitHubApiRepositoriesService gitHubApiRepositoriesService) : IJob
{
	static readonly WeakEventManager<bool> _jobCompletedEventManager = new();

	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly GitHubUserService _gitHubUserService = gitHubUserService;
	readonly RepositoryDatabase _repositoryDatabase = repositoryDatabase;
	readonly NotificationService _notificationService = notificationService;
	readonly GitHubGraphQLApiService _gitHubGraphQLApiService = gitHubGraphQlApiService;
	readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService = gitHubApiRepositoriesService;

	public static event EventHandler<bool> JobCompleted
	{
		add => _jobCompletedEventManager.AddEventHandler(value);
		remove => _jobCompletedEventManager.RemoveEventHandler(value);
	}

	public string NotifyTrendingRepositoriesIdentifier { get; } = $"{appInfo.PackageName}.{nameof(NotifyTrendingRepositoriesJob)}";

	public JobInfo GetJobInfo(bool shouldRunInForeground) => new(
		NotifyTrendingRepositoriesIdentifier,
		typeof(NotifyTrendingRepositoriesJob),
		shouldRunInForeground,
		RequiredInternetAccess: InternetAccess.Unmetered);

	public async Task Run(JobInfo jobInfo, CancellationToken cancellationToken)
	{
		_analyticsService.Track($"{nameof(NotifyTrendingRepositoriesJob)} Triggered");

		try
		{
			if (!_gitHubUserService.IsAuthenticated || _gitHubUserService.IsDemoUser)
			{
				OnScheduleNotifyTrendingRepositoriesCompleted(false);
			}
			else
			{
				var trendingRepositories = await GetTrendingRepositories(cancellationToken).ConfigureAwait(false);
				await _notificationService.TrySendTrendingNotification(trendingRepositories).ConfigureAwait(false);

				OnScheduleNotifyTrendingRepositoriesCompleted(true);
			}
		}
		catch (Exception e)
		{
			_analyticsService.Report(e);
			OnScheduleNotifyTrendingRepositoriesCompleted(false);
		}
	}

	async Task<IReadOnlyList<Repository>> GetTrendingRepositories(CancellationToken cancellationToken)
	{
		if (_gitHubUserService.IsDemoUser || string.IsNullOrEmpty(_gitHubUserService.Alias))
			return [];

		var repositoriesFromDatabase = await _repositoryDatabase.GetRepositories(cancellationToken).ConfigureAwait(false);
		IReadOnlyList<string> favoriteRepositoryUrls = [.. repositoriesFromDatabase.Where(static x => x.IsFavorite is true).Select(static x => x.Url)];

		var retrievedRepositoryList = new List<Repository>();
		await foreach (var repository in _gitHubGraphQLApiService.GetRepositories(_gitHubUserService.Alias, cancellationToken).ConfigureAwait(false))
		{
			if (favoriteRepositoryUrls.Contains(repository.Url))
				retrievedRepositoryList.Add(repository with
				{
					IsFavorite = true
				});
			else
				retrievedRepositoryList.Add(repository);
		}

		var retrievedRepositoryList_NoDuplicatesNoForks = retrievedRepositoryList.RemoveForksDuplicatesAndArchives(static x => x.ContainsViewsClonesData);

		IReadOnlyList<Repository> repositoriesToUpdate =
		[
			.. repositoriesFromDatabase.Where(x => _gitHubUserService.ShouldIncludeOrganizations || x.OwnerLogin == _gitHubUserService.Alias) // Only include organization repositories if `ShouldIncludeOrganizations` is true
				.Where(static x => x.DataDownloadedAt < DateTimeOffset.Now.Subtract(TimeSpan.FromHours(12))) // Cached repositories that haven't been updated in 12 hours 
				.Concat(retrievedRepositoryList_NoDuplicatesNoForks) // Add downloaded repositories
				.GroupBy(static x => x.Name).Select(static x => x.FirstOrDefault(static x => x.ContainsViewsClonesStarsData) ?? x.First())
		]; // Remove duplicate repositories


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

	void OnScheduleNotifyTrendingRepositoriesCompleted(in bool result) =>
		_jobCompletedEventManager.RaiseEvent(this, result, nameof(JobCompleted));
}