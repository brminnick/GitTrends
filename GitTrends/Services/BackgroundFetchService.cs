using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using GitTrends.Shared;
using Shiny;
using Shiny.Jobs;
using Xamarin.Essentials;

namespace GitTrends
{
    public class BackgroundFetchService
    {
        readonly AnalyticsService _analyticsService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly RepositoryDatabase _repositoryDatabase;
        readonly NotificationService _notificationService;

        public BackgroundFetchService(AnalyticsService analyticsService,
                                        GitHubApiV3Service gitHubApiV3Service,
                                        GitHubGraphQLApiService gitHubGraphQLApiService,
                                        RepositoryDatabase repositoryDatabase,
                                        NotificationService notificationService)
        {
            _analyticsService = analyticsService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _repositoryDatabase = repositoryDatabase;
            _notificationService = notificationService;
        }

        static IJobManager JobManager => ShinyHost.Resolve<IJobManager>();

        public void Register()
        {
            var backgroundFetchJob = new JobInfo(typeof(TrendingRepositoryNotificationJob), TrendingRepositoryNotificationJob.Identifier)
            {
                BatteryNotLow = true,
                PeriodicTime = TimeSpan.FromHours(12),
                Repeat = true,
                RequiredInternetAccess = InternetAccess.Any,
                RunOnForeground = false
            };

            JobManager.Schedule(backgroundFetchJob).SafeFireAndForget(ex => _analyticsService.Report(ex));
        }

        public async Task<bool> NotifyTrendingRepositories(CancellationToken cancellationToken)
        {
            try
            {
                using var timedEvent = _analyticsService.TrackTime("Notify Trending Repository Background Job Triggered");

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

        async Task<List<Repository>> GetTrendingRepositories(CancellationToken cancellationToken)
        {
#if AppStore
            if (!GitHubAuthenticationService.IsDemoUser && !string.IsNullOrEmpty(GitHubAuthenticationService.Alias))
#else
            if (!string.IsNullOrEmpty(GitHubAuthenticationService.Alias))
#endif
            {
                var retrievedRepositoryList = new List<Repository>();
                await foreach (var retrievedRepositories in _gitHubGraphQLApiService.GetRepositories(GitHubAuthenticationService.Alias, cancellationToken).ConfigureAwait(false))
                {
                    retrievedRepositoryList.AddRange(retrievedRepositories);
                }

                var retrievedRepositoryList_NoForksOrDuplicates = RepositoryService.RemoveForksAndDuplicates(retrievedRepositoryList).ToList();

                var trendingRepositories = new List<Repository>();
                await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiV3Service.UpdateRepositoriesWithViewsAndClonesData(retrievedRepositoryList_NoForksOrDuplicates, cancellationToken).ConfigureAwait(false))
                {
                    _repositoryDatabase.SaveRepository(retrievedRepositoryWithViewsAndClonesData).SafeFireAndForget();

                    if (retrievedRepositoryWithViewsAndClonesData.IsTrending)
                        trendingRepositories.Add(retrievedRepositoryWithViewsAndClonesData);
                }

                return trendingRepositories;
            }

            return Enumerable.Empty<Repository>().ToList();
        }
    }

    public class TrendingRepositoryNotificationJob : IJob
    {
        public const string Identifier = nameof(TrendingRepositoryNotificationJob);

        public Task<bool> Run(JobInfo jobInfo, CancellationToken cancellationToken)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();

            var backgroundFetchService = scope.Resolve<BackgroundFetchService>();
            return backgroundFetchService.NotifyTrendingRepositories(cancellationToken);
        }
    }
}
