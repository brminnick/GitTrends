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

namespace GitTrends
{
    class BackgroundFetchService
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

        public Task Register()
        {
            var backgroundFetchJob = new JobInfo(typeof(TrendingRepositoryNotificationJob), nameof(TrendingRepositoryNotificationJob))
            {
               
            };

            return ShinyHost.Resolve<IJobManager>().Schedule(backgroundFetchJob);
        }

        public async Task<bool> ExecuteTrendingRepositoryNotificationJob(JobInfo jobInfo, CancellationToken cancelToken)
        {
            try
            {
                using var timedEvent = _analyticsService.TrackTime("Trending Repository Notification Job Triggered");
                var trendingRepositories = await GetTrendingRepositories().ConfigureAwait(false);
                await _notificationService.TrySendTrendingNotificaiton(trendingRepositories).ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                _analyticsService.Report(e);
                return false;
            }
        }

        async Task<List<Repository>> GetTrendingRepositories()
        {
            if (!GitHubAuthenticationService.IsDemoUser && !string.IsNullOrEmpty(GitHubAuthenticationService.Alias))
            {
                var retrievedRepositoryList = new List<Repository>();
                await foreach (var retrievedRepositories in _gitHubGraphQLApiService.GetRepositories(GitHubAuthenticationService.Alias).ConfigureAwait(false))
                {
                    retrievedRepositoryList.AddRange(retrievedRepositories);
                }

                var trendingRepositories = new List<Repository>();
                await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiV3Service.UpdateRepositoriesWithViewsAndClonesData(retrievedRepositoryList).ConfigureAwait(false))
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
        public Task<bool> Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            return scope.Resolve<BackgroundFetchService>().ExecuteTrendingRepositoryNotificationJob(jobInfo, cancelToken);
        }
    }
}
