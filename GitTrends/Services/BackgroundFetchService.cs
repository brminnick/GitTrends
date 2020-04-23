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

            _notificationService.RegisterForNotificationsCompleted += HandleRegisterForNotificationsCompleted;
        }

        static IJobManager JobManager => ShinyHost.Resolve<IJobManager>();

        public Task Register() => MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var periodicTimeSpan = TimeSpan.FromHours(12);

            var isRegistered = await isTrendingRepositoryNotificationJobRegistered();

            if (!isRegistered)
            {
                var backgroundFetchJob = new JobInfo(typeof(TrendingRepositoryNotificationJob), TrendingRepositoryNotificationJob.Identifier)
                {
                    BatteryNotLow = true,
                    PeriodicTime = periodicTimeSpan,
                    Repeat = true,
                    RequiredInternetAccess = InternetAccess.Any,
                    RunOnForeground = false
                };

                await JobManager.Schedule(backgroundFetchJob).ConfigureAwait(false);
            }

            static async Task<bool> isTrendingRepositoryNotificationJobRegistered()
            {
                var registeredJobs = await JobManager.GetJobs().ConfigureAwait(false);
                return registeredJobs.Any(x => x.Identifier is TrendingRepositoryNotificationJob.Identifier);
            }
        });

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

        async void HandleRegisterForNotificationsCompleted(object sender, (bool isSuccessful, string errorMessage) e)
        {
            if (e.isSuccessful)
                await Register().ConfigureAwait(false);
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
