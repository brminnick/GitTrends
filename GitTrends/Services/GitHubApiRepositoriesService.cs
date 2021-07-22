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
        readonly static WeakEventManager<(Repository Repository, TimeSpan RetryTimeSpan)> _abuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsDataEventManager = new();

        readonly IAnalyticsService _analyticsService;
        readonly GitHubUserService _gitHubUserService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly GitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GitHubApiRepositoriesService(IAnalyticsService analyticsService,
                                            GitHubUserService gitHubUserService,
                                            GitHubApiV3Service gitHubApiV3Service,
                                            GitHubApiStatusService gitHubApiStatusService,
                                            GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _analyticsService = analyticsService;
            _gitHubUserService = gitHubUserService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        public static event EventHandler<(Repository Repository, TimeSpan RetryTimeSpan)> AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData
        {
            add => _abuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsDataEventManager.AddEventHandler(value);
            remove => _abuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsDataEventManager.RemoveEventHandler(value);
        }

        public async IAsyncEnumerable<Repository> UpdateRepositoriesWithViewsClonesAndStarsData(IReadOnlyList<Repository> repositories, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var getRepositoryStatisticsTaskList = new List<Task<(RepositoryViewsResponseModel?, RepositoryClonesResponseModel?, StarGazers?)>>(repositories.Select(x => GetRepositoryStatistics(x, cancellationToken)));

            while (getRepositoryStatisticsTaskList.Any())
            {
                var completedStatisticsTask = await Task.WhenAny(getRepositoryStatisticsTaskList).ConfigureAwait(false);
                getRepositoryStatisticsTaskList.Remove(completedStatisticsTask);

                var (viewsResponse, clonesResponse, starGazers) = await completedStatisticsTask.ConfigureAwait(false);

                if (viewsResponse != null && clonesResponse != null && starGazers != null)
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
            var getStarGazrsTask = _gitHubGraphQLApiService.GetStarGazers(repository.Name, repository.OwnerLogin, cancellationToken);
            var getViewStatisticsTask = _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name, cancellationToken);
            var getCloneStatisticsTask = _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name, cancellationToken);

            try
            {
                await Task.WhenAll(getViewStatisticsTask, getCloneStatisticsTask, getStarGazrsTask).ConfigureAwait(false);

                return (await getViewStatisticsTask.ConfigureAwait(false),
                        await getCloneStatisticsTask.ConfigureAwait(false),
                        await getStarGazrsTask.ConfigureAwait(false));
            }
            catch (ApiException e) when (_gitHubApiStatusService.IsAbuseRateLimit(e.Headers, out var timespan) && timespan is TimeSpan retryTimeSpan)
            {
                OnAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsDataEventManager(repository, retryTimeSpan);

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

        void OnAbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsDataEventManager(in Repository repository, in TimeSpan retryTimeSpan) =>
            _abuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsDataEventManager.RaiseEvent(this, (repository, retryTimeSpan), nameof(AbuseRateLimitFound_UpdateRepositoriesWithViewsClonesAndStarsData));
    }
}