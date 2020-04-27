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

        public static string NotifyTrendingRepositoriesIdentifier { get; } = $"{Xamarin.Essentials.AppInfo.PackageName}.{nameof(NotifyTrendingRepositories)}";

        public async Task<bool> Run(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                using var timedEvent = scope.Resolve<AnalyticsService>().TrackTime($"{nameof(NotifyTrendingRepositories)} Triggered");

                var backgroundFetchService = scope.Resolve<BackgroundFetchService>();
                return await backgroundFetchService.NotifyTrendingRepositories(cancellationToken).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                _analyticsService.Report(e);
                return false;
            }
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

        async Task<IReadOnlyList<Repository>> GetTrendingRepositories(CancellationToken cancellationToken)
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

                var trendingRepositories = new List<Repository>();
                await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiV3Service.UpdateRepositoriesWithViewsAndClonesData(retrievedRepositoryList, cancellationToken ).ConfigureAwait(false))
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
}
