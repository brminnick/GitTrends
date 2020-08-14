using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;

namespace GitTrends
{
    public class BackgroundFetchService
    {
        readonly IAnalyticsService _analyticsService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly RepositoryDatabase _repositoryDatabase;
        readonly NotificationService _notificationService;
        readonly ReferringSitesDatabase _referringSitesDatabase;
        readonly GitHubUserService _gitHubUserService;

        public BackgroundFetchService(IAnalyticsService analyticsService,
                                        GitHubApiV3Service gitHubApiV3Service,
                                        GitHubGraphQLApiService gitHubGraphQLApiService,
                                        RepositoryDatabase repositoryDatabase,
                                        ReferringSitesDatabase referringSitesDatabase,
                                        NotificationService notificationService,
                                        GitHubUserService gitHubUserService)
        {
            _analyticsService = analyticsService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _repositoryDatabase = repositoryDatabase;
            _notificationService = notificationService;
            _referringSitesDatabase = referringSitesDatabase;
            _gitHubUserService = gitHubUserService;
        }

        public static string NotifyTrendingRepositoriesIdentifier { get; } = $"{Xamarin.Essentials.AppInfo.PackageName}.{nameof(NotifyTrendingRepositories)}";
        public static string CleanUpDatabaseIdentifier { get; } = $"{Xamarin.Essentials.AppInfo.PackageName}.{nameof(CleanUpDatabase)}";

        public async Task CleanUpDatabase()
        {
            using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(CleanUpDatabase)} Triggered");

            await Task.WhenAll(_referringSitesDatabase.DeleteExpiredData(), _repositoryDatabase.DeleteExpiredData()).ConfigureAwait(false);
        }

        public async Task<bool> NotifyTrendingRepositories(CancellationToken cancellationToken)
        {
            try
            {
                using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(NotifyTrendingRepositories)} Triggered");

                if (!_gitHubUserService.IsAuthenticated || _gitHubUserService.IsDemoUser)
                    return false;

                var trendingRepositories = await GetTrendingRepositories(cancellationToken).ConfigureAwait(false);
                await _notificationService.TrySendTrendingNotificaiton(trendingRepositories).ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                _analyticsService.Report(e);
                return false;
            }
        }

        async Task<IReadOnlyList<Repository>> GetTrendingRepositories(CancellationToken cancellationToken)
        {
            if (!_gitHubUserService.IsDemoUser && !string.IsNullOrEmpty(_gitHubUserService.Alias))
            {
                var retrievedRepositoryList = new List<Repository>();
                await foreach (var retrievedRepositories in _gitHubGraphQLApiService.GetRepositories(_gitHubUserService.Alias, cancellationToken).ConfigureAwait(false))
                {
                    retrievedRepositoryList.AddRange(retrievedRepositories);
                }

                var retrievedRepositoryList_NoDuplicatesNoForks = RepositoryService.RemoveForksAndDuplicates(retrievedRepositoryList);

                var trendingRepositories = new List<Repository>();
                await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiV3Service.UpdateRepositoriesWithViewsAndClonesData(retrievedRepositoryList_NoDuplicatesNoForks.ToList(), cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        await _repositoryDatabase.SaveRepository(retrievedRepositoryWithViewsAndClonesData).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _analyticsService.Report(e);
                    }

                    if (retrievedRepositoryWithViewsAndClonesData.IsTrending)
                        trendingRepositories.Add(retrievedRepositoryWithViewsAndClonesData);
                }

                return trendingRepositories;
            }

            return new List<Repository>();
        }
    }
}
