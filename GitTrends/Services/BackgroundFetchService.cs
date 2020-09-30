using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class BackgroundFetchService
    {
        readonly IAnalyticsService _analyticsService;
        readonly GitHubUserService _gitHubUserService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly RepositoryDatabase _repositoryDatabase;
        readonly NotificationService _notificationService;
        readonly ReferringSitesDatabase _referringSitesDatabase;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService;

        public BackgroundFetchService(IAppInfo appInfo,
                                        GitHubUserService gitHubUserService,
                                        IAnalyticsService analyticsService,
                                        GitHubApiV3Service gitHubApiV3Service,
                                        RepositoryDatabase repositoryDatabase,
                                        NotificationService notificationService,
                                        ReferringSitesDatabase referringSitesDatabase,
                                        GitHubGraphQLApiService gitHubGraphQLApiService,
                                        GitHubApiRepositoriesService gitHubApiRepositoriesService)
        {
            _analyticsService = analyticsService;
            _gitHubUserService = gitHubUserService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _repositoryDatabase = repositoryDatabase;
            _notificationService = notificationService;
            _referringSitesDatabase = referringSitesDatabase;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _gitHubApiRepositoriesService = gitHubApiRepositoriesService;

            CleanUpDatabaseIdentifier = $"{appInfo.PackageName}.{nameof(CleanUpDatabase)}";
            NotifyTrendingRepositoriesIdentifier = $"{appInfo.PackageName}.{nameof(NotifyTrendingRepositories)}";
        }

        public string CleanUpDatabaseIdentifier { get; }
        public string NotifyTrendingRepositoriesIdentifier { get; }

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
                await foreach (var repository in _gitHubGraphQLApiService.GetRepositories(_gitHubUserService.Alias, cancellationToken).ConfigureAwait(false))
                {
                    retrievedRepositoryList.Add(repository);
                }

                var retrievedRepositoryList_NoDuplicatesNoForks = RepositoryService.RemoveForksAndDuplicates(retrievedRepositoryList);

                var trendingRepositories = new List<Repository>();
                await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(retrievedRepositoryList_NoDuplicatesNoForks.ToList(), cancellationToken).ConfigureAwait(false))
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

            return Array.Empty<Repository>();
        }
    }
}
