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
        readonly static WeakEventManager _eventManager = new();
        readonly static WeakEventManager<bool> _scheduleNotifyTrendingRepositoriesCompletedEventManager = new();
        readonly static WeakEventManager<string> _scheduleRetryOrganizationsRepositoriesCompletedEventManager = new();
        readonly static WeakEventManager<Repository> _scheduleRetryRepositoriesViewsClonesEventManager = new();
        readonly static WeakEventManager<MobileReferringSiteModel> _scheduleRetryGetReferringSiteCompletedEventManager = new();

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
            RetryGetReferringSitesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryGetReferringSites)}";
            NotifyTrendingRepositoriesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleNotifyTrendingRepositories)}";
            RetryRepositoriesViewsClonesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryRepositoriesViewsClones)}";
            RetryRetryOrganizationsReopsitoriesIdentifier = $"{appInfo.PackageName}.{nameof(ScheduleRetryOrganizationsRepositories)}";

            GitHubApiRepositoriesService.AbuseRateLimitFound_GetReferringSites += HandleAbuseRateLimitFound_GetReferringSites;
            GitHubGraphQLApiService.AbuseRateLimitFound_GetOrganizationRepositories += HandleAbuseRateLimitFound_GetOrganizationRepositories;
            GitHubApiRepositoriesService.AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData += HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData;
        }

        public static event EventHandler DatabaseCleanupCompleted
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<MobileReferringSiteModel> ScheduleRetryGetReferringSiteCompleted
        {
            add => _scheduleRetryGetReferringSiteCompletedEventManager.AddEventHandler(value);
            remove => _scheduleRetryGetReferringSiteCompletedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<bool> ScheduleNotifyTrendingRepositoriesCompleted
        {
            add => _scheduleNotifyTrendingRepositoriesCompletedEventManager.AddEventHandler(value);
            remove => _scheduleNotifyTrendingRepositoriesCompletedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<string> ScheduleRetryOrganizationsRepositoriesCompleted
        {
            add => _scheduleRetryOrganizationsRepositoriesCompletedEventManager.AddEventHandler(value);
            remove => _scheduleRetryOrganizationsRepositoriesCompletedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<Repository> ScheduleRetryRepositoriesViewsClonesCompleted
        {
            add => _scheduleRetryRepositoriesViewsClonesEventManager.AddEventHandler(value);
            remove => _scheduleRetryRepositoriesViewsClonesEventManager.RemoveEventHandler(value);
        }


        public string CleanUpDatabaseIdentifier { get; }
        public string RetryGetReferringSitesIdentifier { get; }
        public string NotifyTrendingRepositoriesIdentifier { get; }
        public string RetryRepositoriesViewsClonesIdentifier { get; }
        public string RetryRetryOrganizationsReopsitoriesIdentifier { get; }

        public void Initialize()
        {
            //Required to ensure the service is instantiated in the Dependency Injection Container and event in the constructor are subscribed
            var temp = DateTime.UtcNow;
        }

        public void ScheduleRetryOrganizationsRepositories(string organizationName, TimeSpan? delay = null) => _jobManager.RunTask(RetryRetryOrganizationsReopsitoriesIdentifier, async cancellationToken =>
        {
            _analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryOrganizationsRepositories)} Triggered", nameof(delay), delay?.ToString() ?? "null");

            if (delay is not null)
                await Task.Delay(delay.Value).ConfigureAwait(false);

            var repositories = await _gitHubGraphQLApiService.GetOrganizationRepositories(organizationName, cancellationToken).ConfigureAwait(false);

            foreach (var repository in repositories)
                ScheduleRetryRepositoriesViewsClones(repository);

            OnScheduleRetryOrganizationsRepositoriesCompleted(organizationName);
        });

        public void ScheduleRetryRepositoriesViewsClones(Repository repository, TimeSpan? delay = null) => _jobManager.RunTask(RetryRepositoriesViewsClonesIdentifier, async cancellationToken =>
        {
            _analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryRepositoriesViewsClones)} Triggered", nameof(delay), delay?.ToString() ?? "null");

            if (delay is not null)
                await Task.Delay(delay.Value).ConfigureAwait(false);

            using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryRepositoriesViewsClones)} Executed");

            await foreach (var repository in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(new List<Repository> { repository }, cancellationToken).ConfigureAwait(false))
            {
                if (repository is not null)
                {
                    await _repositoryDatabase.SaveRepository(repository).ConfigureAwait(false);

                    OnScheduleRetryRepositoriesViewsClonesCompleted(repository);
                }
            }
        });

        public void ScheduleRetryGetReferringSites(Repository repository, TimeSpan? delay = null) => _jobManager.RunTask(RetryGetReferringSitesIdentifier, async cancellationToken =>
        {
            _analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(ScheduleRetryGetReferringSites)} Triggered", nameof(delay), delay?.ToString() ?? "null");

            if (delay is not null)
                await Task.Delay(delay.Value).ConfigureAwait(false);

            try
            {
                var referringSites = await _gitHubApiRepositoriesService.GetReferringSites(repository, cancellationToken).ConfigureAwait(false);

                await foreach (var mobileReferringSite in _gitHubApiRepositoriesService.GetMobileReferringSites(referringSites, repository.Url, CancellationToken.None))
                {
                    await _referringSitesDatabase.SaveReferringSite(mobileReferringSite, repository.Url).ConfigureAwait(false);
                    OnScheduleRetryGetReferringSitesCompleted(mobileReferringSite);
                }
            }
            catch (Exception e)
            {
                _analyticsService.Report(e);
            }
        });

        public void ScheduleCleanUpDatabase() => _jobManager.RunTask(CleanUpDatabaseIdentifier, async cancellationToken =>
        {
            using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleCleanUpDatabase)} Triggered");

            await Task.WhenAll(_referringSitesDatabase.DeleteExpiredData(), _repositoryDatabase.DeleteExpiredData()).ConfigureAwait(false);

            OnDatabaseCleanupCompleted();
        });

        public void ScheduleNotifyTrendingRepositories(CancellationToken cancellationToken) => _jobManager.RunTask(NotifyTrendingRepositoriesIdentifier, async cancellationToken =>
        {
            try
            {
                using var timedEvent = _analyticsService.TrackTime($"{nameof(BackgroundFetchService)}.{nameof(ScheduleNotifyTrendingRepositories)} Triggered");

                if (!_gitHubUserService.IsAuthenticated || _gitHubUserService.IsDemoUser)
                {
                    OnScheduleNotifyTrendingRepositoriesCompleted(false);
                }
                else
                {
                    var trendingRepositories = await GetTrendingRepositories(cancellationToken).ConfigureAwait(false);
                    await _notificationService.TrySendTrendingNotificaiton(trendingRepositories).ConfigureAwait(false);

                    OnScheduleNotifyTrendingRepositoriesCompleted(true);
                }
            }
            catch (Exception e)
            {
                _analyticsService.Report(e);
                OnScheduleNotifyTrendingRepositoriesCompleted(false);
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

        void HandleAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData(object sender, (Repository Repository, TimeSpan Delay) data) =>
            ScheduleRetryRepositoriesViewsClones(data.Repository, data.Delay);

        void HandleAbuseRateLimitFound_GetOrganizationRepositories(object sender, (string OrganizationName, TimeSpan RetryTimeSpan) data) =>
            ScheduleRetryOrganizationsRepositories(data.OrganizationName, data.RetryTimeSpan);

        void HandleAbuseRateLimitFound_GetReferringSites(object sender, (Repository Repository, TimeSpan RetryTimeSpan) data) =>
            ScheduleRetryGetReferringSites(data.Repository, data.RetryTimeSpan);

        void OnScheduleRetryRepositoriesViewsClonesCompleted(in Repository repository) =>
            _scheduleRetryRepositoriesViewsClonesEventManager.RaiseEvent(this, repository, nameof(ScheduleRetryRepositoriesViewsClonesCompleted));

        void OnDatabaseCleanupCompleted() => _eventManager.RaiseEvent(this, EventArgs.Empty, nameof(DatabaseCleanupCompleted));

        void OnScheduleNotifyTrendingRepositoriesCompleted(in bool result) =>
            _scheduleNotifyTrendingRepositoriesCompletedEventManager.RaiseEvent(this, result, nameof(ScheduleNotifyTrendingRepositoriesCompleted));

        void OnScheduleRetryOrganizationsRepositoriesCompleted(in string organizationName) =>
            _scheduleRetryOrganizationsRepositoriesCompletedEventManager.RaiseEvent(this, organizationName, nameof(ScheduleRetryOrganizationsRepositoriesCompleted));

        void OnScheduleRetryGetReferringSitesCompleted(in MobileReferringSiteModel referringSite) =>
            _scheduleRetryGetReferringSiteCompletedEventManager.RaiseEvent(this, referringSite, nameof(ScheduleRetryGetReferringSiteCompleted));
    }
}
