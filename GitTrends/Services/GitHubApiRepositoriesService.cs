using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitHubApiStatus;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
	public class GitHubApiRepositoriesService
	{
		readonly static WeakEventManager<(Repository Repository, TimeSpan RetryTimeSpan)> _abuseRateLimitFoundEventManager = new();

		readonly FavIconService _favIconService;
		readonly IAnalyticsService _analyticsService;
		readonly GitHubUserService _gitHubUserService;
		readonly GitHubApiV3Service _gitHubApiV3Service;
		readonly RepositoryDatabase _repositoryDatabase;
		readonly ReferringSitesDatabase _referringSitesDatabase;
		readonly GitHubApiStatusService _gitHubApiStatusService;
		readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

		public GitHubApiRepositoriesService(FavIconService favIconService,
											IAnalyticsService analyticsService,
											GitHubUserService gitHubUserService,
											GitHubApiV3Service gitHubApiV3Service,
											RepositoryDatabase repositoryDatabase,
											ReferringSitesDatabase referringSitesDatabase,
											GitHubApiStatusService gitHubApiStatusService,
											GitHubGraphQLApiService gitHubGraphQLApiService)
		{
			_favIconService = favIconService;
			_analyticsService = analyticsService;
			_gitHubUserService = gitHubUserService;
			_gitHubApiV3Service = gitHubApiV3Service;
			_repositoryDatabase = repositoryDatabase;
			_referringSitesDatabase = referringSitesDatabase;
			_gitHubApiStatusService = gitHubApiStatusService;
			_gitHubGraphQLApiService = gitHubGraphQLApiService;
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
			var favIconTaskList = referringSites.Select(x => setFavIcon(_referringSitesDatabase, x, repositoryUrl, cancellationToken)).ToList();

			while (favIconTaskList.Any())
			{
				var completedFavIconTask = await Task.WhenAny(favIconTaskList).ConfigureAwait(false);
				favIconTaskList.Remove(completedFavIconTask);

				var mobileReferringSiteModel = await completedFavIconTask.ConfigureAwait(false);
				yield return mobileReferringSiteModel;
			}

			async Task<MobileReferringSiteModel> setFavIcon(ReferringSitesDatabase referringSitesDatabase, ReferringSiteModel referringSiteModel, string repositoryUrl, CancellationToken cancellationToken)
			{
				var mobileReferringSiteFromDatabase = await referringSitesDatabase.GetReferringSite(repositoryUrl, referringSiteModel.ReferrerUri).ConfigureAwait(false);

				if (mobileReferringSiteFromDatabase != null && isFavIconValid(mobileReferringSiteFromDatabase))
					return mobileReferringSiteFromDatabase;

				if (referringSiteModel.ReferrerUri != null && referringSiteModel.IsReferrerUriValid)
				{
					var favIcon = await _favIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri, cancellationToken).ConfigureAwait(false);
					return new MobileReferringSiteModel(referringSiteModel, favIcon);
				}
				else
				{
					return new MobileReferringSiteModel(referringSiteModel, FavIconService.DefaultFavIcon);
				}

				static bool isFavIconValid(MobileReferringSiteModel mobileReferringSiteModel) => !string.IsNullOrWhiteSpace(mobileReferringSiteModel.FavIconImageUrl) && mobileReferringSiteModel.DownloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(30))) > 0;
			}
		}

		public async IAsyncEnumerable<Repository> UpdateRepositoriesWithViewsClonesAndStarsData(IReadOnlyList<Repository> repositories, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			var getRepositoryStatisticsTaskList = new List<Task<(RepositoryViewsResponseModel?, RepositoryClonesResponseModel?, StarGazers?)>>(repositories.Select(x => GetRepositoryStatistics(x, cancellationToken)));

			while (getRepositoryStatisticsTaskList.Any())
			{
				var completedStatisticsTask = await Task.WhenAny(getRepositoryStatisticsTaskList).ConfigureAwait(false);
				getRepositoryStatisticsTaskList.Remove(completedStatisticsTask);

				var (viewsResponse, clonesResponse, starGazers) = await completedStatisticsTask.ConfigureAwait(false);

				if (starGazers is not null
					&& viewsResponse is not null
					&& clonesResponse is not null)
				{
					var updatedRepository = repositories.Single(x => x.Name == viewsResponse.RepositoryName) with
					{
						DailyViewsList = viewsResponse.DailyViewsList,
						DailyClonesList = clonesResponse.DailyClonesList,
						StarredAt = starGazers.StarredAt.Select(x => x.StarredAt).ToList()
					};

					yield return updatedRepository;
				}
			}
		}

		async Task<(RepositoryViewsResponseModel? ViewsResponse, RepositoryClonesResponseModel? ClonesResponse, StarGazers? StarGazerResponse)> GetRepositoryStatistics(Repository repository, CancellationToken cancellationToken)
		{
			var getStarGazrsTask = _gitHubApiV3Service.GetStarGazers(repository.Name, repository.OwnerLogin, cancellationToken);
			var getViewStatisticsTask = _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name, cancellationToken);
			var getCloneStatisticsTask = _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name, cancellationToken);

			try
			{
				await Task.WhenAll(getViewStatisticsTask, getCloneStatisticsTask, getStarGazrsTask).ConfigureAwait(false);

				return (await getViewStatisticsTask.ConfigureAwait(false),
						await getCloneStatisticsTask.ConfigureAwait(false),
						await getStarGazrsTask.ConfigureAwait(false));
			}
			catch (ApiException e) when (_gitHubApiStatusService.IsAbuseRateLimit(e.Headers, out var timespan))
			{
				OnAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(repository, timespan.Value);

				return (null, null, null);
			}
			catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Forbidden)
			{
				reportException(e);

				return (null, null, null);
			}
			catch (GraphQLException<StarGazers> e) when (e.ContainsSamlOrganizationAthenticationError(out _))
			{
				reportException(e);

				return (null, null, null);
			}
			catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound) // Repository deleted from GitHub but has not yet been deleted from local SQLite Database
			{
				reportException(e);

				var repositoryFromDatabase = await _repositoryDatabase.GetRepository(repository.Url).ConfigureAwait(false);
				if (repositoryFromDatabase is null)
					throw;

				await _repositoryDatabase.DeleteRepository(repository).ConfigureAwait(false);
				return (null, null, null);
			}

			void reportException(in Exception e)
			{
				_analyticsService.Report(e, new Dictionary<string, string>
				{
					{ nameof(Repository) + nameof(Repository.Name), repository.Name },
					{ nameof(Repository) + nameof(Repository.OwnerLogin), repository.OwnerLogin },
					{ nameof(GitHubUserService) + nameof(GitHubUserService.Alias), _gitHubUserService.Alias },
					{ nameof(GitHubUserService) + nameof(GitHubUserService.Name), _gitHubUserService.Name },
					{ nameof(GitHubApiStatusService) + nameof(GitHubApiStatusService.IsAbuseRateLimit),  _gitHubApiStatusService.IsAbuseRateLimit(e, out _).ToString() }
				});
			}
		}

		void OnAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(in Repository repository, in TimeSpan retryTimeSpan) =>
			_abuseRateLimitFoundEventManager.RaiseEvent(this, (repository, retryTimeSpan), nameof(AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData));

		void OnAbuseRateLimitFound_GetReferringSites(in Repository repository, in TimeSpan retryTimeSpan) =>
			_abuseRateLimitFoundEventManager.RaiseEvent(this, (repository, retryTimeSpan), nameof(AbuseRateLimitFound_GetReferringSites));
	}
}