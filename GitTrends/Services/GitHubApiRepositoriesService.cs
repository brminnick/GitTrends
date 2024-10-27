using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;
using GitHubApiStatus;
using GitTrends.Common;
using Refit;

namespace GitTrends;

public class GitHubApiRepositoriesService(
	FavIconService favIconService,
	IAnalyticsService analyticsService,
	GitHubUserService gitHubUserService,
	GitHubApiV3Service gitHubApiV3Service,
	ReferringSitesDatabase referringSitesDatabase,
	IGitHubApiStatusService gitHubApiStatusService,
	GitHubGraphQLApiService gitHubGraphQLApiService)
{
	static readonly WeakEventManager<(Repository Repository, TimeSpan RetryTimeSpan)> _abuseRateLimitFoundEventManager = new();
	static readonly WeakEventManager<Uri> _repositoryUriNotFoundEventManager = new();

	readonly FavIconService _favIconService = favIconService;
	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly GitHubUserService _gitHubUserService = gitHubUserService;
	readonly GitHubApiV3Service _gitHubApiV3Service = gitHubApiV3Service;
	readonly ReferringSitesDatabase _referringSitesDatabase = referringSitesDatabase;
	readonly IGitHubApiStatusService _gitHubApiStatusService = gitHubApiStatusService;
	readonly GitHubGraphQLApiService _gitHubGraphQLApiService = gitHubGraphQLApiService;

	public static event EventHandler<Uri> RepositoryUriNotFound
	{
		add => _repositoryUriNotFoundEventManager.AddEventHandler(value);
		remove => _repositoryUriNotFoundEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<(Repository Repository, TimeSpan RetryTimeSpan)> AbuseRateLimitFound_GetReferringSites
	{
		add => _abuseRateLimitFoundEventManager.AddEventHandler(value);
		remove => _abuseRateLimitFoundEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<(Repository Repository, TimeSpan RetryTimeSpan)> AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData
	{
		add => _abuseRateLimitFoundEventManager.AddEventHandler(value);
		remove => _abuseRateLimitFoundEventManager.RemoveEventHandler(value);
	}

	public async Task<IReadOnlyList<ReferringSiteModel>> GetReferringSites(Repository repository, CancellationToken cancellationToken)
	{
		try
		{
			return await _gitHubApiV3Service.GetReferringSites(repository.OwnerLogin, repository.Name, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception e) when (_gitHubApiStatusService.IsAbuseRateLimit(e, out var retryDelay))
		{
			OnAbuseRateLimitFound_GetReferringSites(repository, retryDelay.Value);
			throw;
		}
	}

	public async IAsyncEnumerable<MobileReferringSiteModel> GetMobileReferringSites(IEnumerable<ReferringSiteModel> referringSites, string repositoryUrl, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		List<Task<MobileReferringSiteModel>> favIconTaskList = [.. referringSites.Select(x => setFavIcon(_referringSitesDatabase, _favIconService, x, repositoryUrl, cancellationToken))];

		while (favIconTaskList.Count is not 0)
		{
			var completedFavIconTask = await Task.WhenAny(favIconTaskList).ConfigureAwait(false);
			favIconTaskList.Remove(completedFavIconTask);

			var mobileReferringSiteModel = await completedFavIconTask.ConfigureAwait(false);
			yield return mobileReferringSiteModel;
		}

		static async Task<MobileReferringSiteModel> setFavIcon(ReferringSitesDatabase referringSitesDatabase, FavIconService favIconService, ReferringSiteModel referringSiteModel, string repositoryUrl, CancellationToken cancellationToken)
		{
			var mobileReferringSiteFromDatabase = await referringSitesDatabase.GetReferringSite(repositoryUrl, referringSiteModel.ReferrerUri, cancellationToken).ConfigureAwait(false);

			if (mobileReferringSiteFromDatabase is not null && isFavIconValid(mobileReferringSiteFromDatabase))
				return mobileReferringSiteFromDatabase;

			if (referringSiteModel.ReferrerUri is not null && referringSiteModel.IsReferrerUriValid)
			{
				var favIcon = await favIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri, cancellationToken).ConfigureAwait(false);
				return new MobileReferringSiteModel(referringSiteModel, favIcon);
			}
			else
			{
				return new MobileReferringSiteModel(referringSiteModel, FavIconService.DefaultFavIcon);
			}

			static bool isFavIconValid(MobileReferringSiteModel mobileReferringSiteModel) => !string.IsNullOrWhiteSpace(mobileReferringSiteModel.FavIconImageUrl) && mobileReferringSiteModel.DownloadedAt > DateTimeOffset.UtcNow.Subtract(CachedDataConstants.DatabaseReferringSitesLifeSpan);
		}
	}

	public async IAsyncEnumerable<Repository> UpdateRepositoriesWithViewsAndClonesData(IEnumerable<Repository> repositories, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		repositories = repositories.ToList();

		var getRepositoryStatisticsTaskList = new List<Task<(RepositoryViewsModel?, RepositoryClonesModel?)>>(repositories.Select(x => GetViewsAndClonesStatistics(x, cancellationToken)));

		while (getRepositoryStatisticsTaskList.Any())
		{
			var completedStatisticsTask = await Task.WhenAny(getRepositoryStatisticsTaskList).ConfigureAwait(false);
			getRepositoryStatisticsTaskList.Remove(completedStatisticsTask);

			var (viewsResponse, clonesResponse) = await completedStatisticsTask.ConfigureAwait(false);

			if (viewsResponse is not null
				&& clonesResponse is not null)
			{
				var updatedRepository = repositories.Single(x => x.Name == viewsResponse.RepositoryName) with
				{
					DailyViewsList = viewsResponse.DailyViewsList,
					DailyClonesList = clonesResponse.DailyClonesList
				};

				yield return updatedRepository;
			}
		}
	}

	public async IAsyncEnumerable<Repository> UpdateRepositoriesWithStarsData(IEnumerable<Repository> repositories, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		repositories = repositories.ToList();

		var getRepositoryStatisticsTaskList = new List<Task<(string RepositoryName, StarGazers? StarGazers)>>(repositories.Select(x => GetStarGazersStatistics(x, cancellationToken)));

		while (getRepositoryStatisticsTaskList.Count is not 0)
		{
			var completedStatisticsTask = await Task.WhenAny(getRepositoryStatisticsTaskList).ConfigureAwait(false);
			getRepositoryStatisticsTaskList.Remove(completedStatisticsTask);

			var starsResponse = await completedStatisticsTask.ConfigureAwait(false);

			if (starsResponse.StarGazers is not null)
			{
				var updatedRepository = repositories.Single(x => x.Name == starsResponse.RepositoryName) with
				{
					StarredAt = [.. starsResponse.StarGazers.StarredAt.Select(static x => x.StarredAt)]
				};

				yield return updatedRepository;
			}
		}
	}

	async Task<(RepositoryViewsModel? ViewsResponse, RepositoryClonesModel? ClonesResponse)> GetViewsAndClonesStatistics(Repository repository, CancellationToken cancellationToken)
	{
		var getViewStatisticsTask = _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name, cancellationToken);
		var getCloneStatisticsTask = _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name, cancellationToken);

		try
		{
			await Task.WhenAll(getViewStatisticsTask, getCloneStatisticsTask).ConfigureAwait(false);

			return (await getViewStatisticsTask.ConfigureAwait(false),
				await getCloneStatisticsTask.ConfigureAwait(false));
		}
		catch (ApiException e) when (_gitHubApiStatusService.IsAbuseRateLimit(e.Headers, out var timespan))
		{
			OnAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(repository, timespan.Value);

			return (null, null);
		}
		catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Forbidden)
		{
			reportException(e);

			return (null, null);
		}
		catch (GraphQLException<StarGazers> e) when (e.ContainsSamlOrganizationAuthenticationError(out _))
		{
			reportException(e);

			return (null, null);
		}
		catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound) // Repository deleted from GitHub but has not yet been deleted from local SQLite Database
		{
			reportException(e);

			OnRepositoryUriNotFound(repository.Url);

			return (null, null);
		}

		void reportException(in Exception e)
		{
			_analyticsService.Report(e, new Dictionary<string, string>
			{
				{
					nameof(Repository) + nameof(Repository.Name), repository.Name
				},
				{
					nameof(Repository) + nameof(Repository.OwnerLogin), repository.OwnerLogin
				},
				{
					nameof(GitHubUserService) + nameof(GitHubUserService.Alias), _gitHubUserService.Alias
				},
				{
					nameof(GitHubUserService) + nameof(GitHubUserService.Name), _gitHubUserService.Name
				},
				{
					nameof(GitHubApiStatusService) + nameof(GitHubApiStatusService.IsAbuseRateLimit), _gitHubApiStatusService.IsAbuseRateLimit(e, out _).ToString()
				}
			});
		}
	}

	async Task<(string RepositoryName, StarGazers? StarGazers)> GetStarGazersStatistics(Repository repository, CancellationToken cancellationToken)
	{
		try
		{
			var starGazers = await _gitHubGraphQLApiService.GetStarGazers(repository.Name, repository.OwnerLogin, cancellationToken).ConfigureAwait(false);
			return (repository.Name, starGazers);
		}
		catch (ApiException e) when (_gitHubApiStatusService.IsAbuseRateLimit(e.Headers, out var timespan))
		{
			OnAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(repository, timespan.Value);

			return (repository.Name, null);
		}
		catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Forbidden)
		{
			reportException(e);

			return (repository.Name, null);
		}
		catch (GraphQLException<StarGazers> e) when (e.ContainsSamlOrganizationAuthenticationError(out _))
		{
			reportException(e);

			return (repository.Name, null);
		}
		catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound) // Repository deleted from GitHub but has not yet been deleted from local SQLite Database
		{
			reportException(e);

			OnRepositoryUriNotFound(repository.Url);

			return (repository.Name, null);
		}

		void reportException(in Exception e)
		{
			_analyticsService.Report(e, new Dictionary<string, string>
			{
				{
					nameof(Repository) + nameof(Repository.Name), repository.Name
				},
				{
					nameof(Repository) + nameof(Repository.OwnerLogin), repository.OwnerLogin
				},
				{
					nameof(GitHubUserService) + nameof(GitHubUserService.Alias), _gitHubUserService.Alias
				},
				{
					nameof(GitHubUserService) + nameof(GitHubUserService.Name), _gitHubUserService.Name
				},
				{
					nameof(GitHubApiStatusService) + nameof(GitHubApiStatusService.IsAbuseRateLimit), _gitHubApiStatusService.IsAbuseRateLimit(e, out _).ToString()
				}
			});
		}
	}

	void OnAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(in Repository repository, in TimeSpan retryTimeSpan) =>
		_abuseRateLimitFoundEventManager.RaiseEvent(this, (repository, retryTimeSpan), nameof(AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData));

	void OnAbuseRateLimitFound_GetReferringSites(in Repository repository, in TimeSpan retryTimeSpan) =>
		_abuseRateLimitFoundEventManager.RaiseEvent(this, (repository, retryTimeSpan), nameof(AbuseRateLimitFound_GetReferringSites));

	void OnRepositoryUriNotFound(in string url) =>
		_repositoryUriNotFoundEventManager.RaiseEvent(this, new Uri(url), nameof(RepositoryUriNotFound));
}