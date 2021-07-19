using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Shiny.Jobs;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class BackgroundFetchService
    {
        readonly static WeakEventManager<Repository> _retryUpdateRepositorysWithViewsClonesAndStarsDataCompletedEventManager = new();

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
            NotifyTrendingRepositoriesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleNotifyTrendingRepositories)}";
            RetryRepositoriesViewsClonesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryRepositoriesViewsClones)}";

            GitHubApiRepositoriesService.AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData += HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData;
        }

        public static event EventHandler<Repository> RetryUpdateRepositoryWithViewsClonesAndStarsDataCompleted
        {
            add => _retryUpdateRepositorysWithViewsClonesAndStarsDataCompletedEventManager.AddEventHandler(value);
            remove => _retryUpdateRepositorysWithViewsClonesAndStarsDataCompletedEventManager.RemoveEventHandler(value);
        }

        public string CleanUpDatabaseIdentifier { get; }
        public string NotifyTrendingRepositoriesIdentifier { get; }
        public string RetryRepositoriesViewsClonesIdentifier { get; }

        public async void ScheduleRetryRepositoriesViewsClones(Repository repository, TimeSpan? retryTimeSpan = null)
        {
            if (retryTimeSpan is TimeSpan retry)
                await Task.Delay(retry).ConfigureAwait(false);

            _jobManager.RunTask(RetryRepositoriesViewsClonesIdentifier, async cancellationToken =>
            {
                await foreach (var repository in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(new List<Repository> { repository }, cancellationToken).ConfigureAwait(false))
                {
                    if (repository is not null)
                    {
                        await _repositoryDatabase.SaveRepository(repository).ConfigureAwait(false);

                        OnRetryUpdateRepositoryWithViewsClonesAndStarsDataCompleted(repository);
                    }
                }
            });
        }

        public void ScheduleCleanUpDatabase() => _jobManager.RunTask(CleanUpDatabaseIdentifier, cancellationToken =>
        {
            using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleCleanUpDatabase)} Triggered");

            return Task.WhenAll(_referringSitesDatabase.DeleteExpiredData(), _repositoryDatabase.DeleteExpiredData());
        });

        public void ScheduleNotifyTrendingRepositories(CancellationToken cancellationToken) => _jobManager.RunTask(NotifyTrendingRepositoriesIdentifier, async cancellationToken =>
        {
            try
            {
                using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleNotifyTrendingRepositories)} Triggered");

                if (!_gitHubUserService.IsAuthenticated || _gitHubUserService.IsDemoUser)
                    return;

                var trendingRepositories = await GetTrendingRepositories(cancellationToken).ConfigureAwait(false);
                await _notificationService.TrySendTrendingNotificaiton(trendingRepositories).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _analyticsService.Report(e);
            }
        });

        async Task<IReadOnlyList<Repository>> GetTrendingRepositories(CancellationToken cancellationToken)
        {
            if (!_gitHubUserService.IsDemoUser && !string.IsNullOrEmpty(_gitHubUserService.Alias))
            {
                var favoriteRepositoryUrls = await _repositoryDatabase.GetFavoritesUrls().ConfigureAwait(false);

                var retrievedRepositoryList = new List<Repository>();
                await foreach (var repository in _gitHubGraphQLApiService.GetRepositories(_gitHubUserService.Alias, cancellationToken).ConfigureAwait(false))
                {
                    if (favoriteRepositoryUrls.Contains(repository.Url))
                        retrievedRepositoryList.Add(repository with { IsFavorite = true });
                    else
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

        void HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(object sender, (Repository Repository, TimeSpan TimeSpan) data) =>
            ScheduleRetryRepositoriesViewsClones(data.Repository, data.TimeSpan);

        void OnRetryUpdateRepositoryWithViewsClonesAndStarsDataCompleted(in Repository repository) =>
            _retryUpdateRepositorysWithViewsClonesAndStarsDataCompletedEventManager.RaiseEvent(this, repository, nameof(RetryUpdateRepositoryWithViewsClonesAndStarsDataCompleted));
    }
}
