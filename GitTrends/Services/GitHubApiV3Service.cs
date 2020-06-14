using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubApiV3Service : BaseMobileApiService
    {
        readonly static Lazy<IGitHubApiV3> _githubApiClient = new Lazy<IGitHubApiV3>(() => RestService.For<IGitHubApiV3>(CreateHttpClient(GitHubConstants.GitHubRestApiUrl)));
        readonly GitHubUserService _gitHubUserService;

        public GitHubApiV3Service(IAnalyticsService analyticsService, IMainThread mainThread, GitHubUserService gitHubUserService) : base(analyticsService, mainThread)
        {
            _gitHubUserService = gitHubUserService;
        }

        static IGitHubApiV3 GithubApiClient => _githubApiClient.Value;

        public async IAsyncEnumerable<Repository> UpdateRepositoriesWithViewsAndClonesData(IReadOnlyList<Repository> repositories, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var getRepositoryStatisticsTaskList = new List<Task<(RepositoryViewsResponseModel?, RepositoryClonesResponseModel?)>>(repositories.Select(x => getRepositoryStatistics(x)));

            while (getRepositoryStatisticsTaskList.Any())
            {
                var completedStatisticsTask = await Task.WhenAny(getRepositoryStatisticsTaskList).ConfigureAwait(false);
                getRepositoryStatisticsTaskList.Remove(completedStatisticsTask);

                var (viewsResponse, clonesResponse) = await completedStatisticsTask.ConfigureAwait(false);

                if (viewsResponse != null && clonesResponse != null)
                {
                    var matchingRepository = repositories.Single(x => x.Name == viewsResponse.RepositoryName);

                    yield return new Repository(matchingRepository.Name, matchingRepository.Description, matchingRepository.ForkCount,
                                                new RepositoryOwner(matchingRepository.OwnerLogin, matchingRepository.OwnerAvatarUrl),
                                                new IssuesConnection(matchingRepository.IssuesCount, null),
                                                matchingRepository.Url,
                                                new StarGazers(matchingRepository.StarCount),
                                                matchingRepository.IsFork,
                                                viewsResponse.DailyViewsList,
                                                clonesResponse.DailyClonesList);
                }
            }

            async Task<(RepositoryViewsResponseModel? ViewsResponse, RepositoryClonesResponseModel? ClonesResponse)> getRepositoryStatistics(Repository repository)
            {
                var getViewStatisticsTask = GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name, cancellationToken);
                var getCloneStatisticsTask = GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name, cancellationToken);

                try
                {
                    await Task.WhenAll(getViewStatisticsTask, getCloneStatisticsTask).ConfigureAwait(false);

                    return (await getViewStatisticsTask.ConfigureAwait(false),
                            await getCloneStatisticsTask.ConfigureAwait(false));
                }
                catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Forbidden)
                {
                    AnalyticsService.Report(e, new Dictionary<string, string>
                    {
                        { nameof(Repository)+ nameof(Repository.Name), repository.Name },
                        { nameof(Repository)+ nameof(Repository.OwnerLogin), repository.OwnerLogin },
                        { nameof(GitHubUserService) + nameof(GitHubUserService.Alias), _gitHubUserService.Alias },
                        { nameof(GitHubUserService) + nameof(GitHubUserService.Name), _gitHubUserService.Name },
                    });

                    return (null, null);
                }
            }
        }

        public async Task<RepositoryViewsResponseModel> GetRepositoryViewStatistics(string owner, string repo, CancellationToken cancellationToken)
        {
            if (_gitHubUserService.IsDemoUser)
            {
                //Yield off of the main thread to generate dailyViewsModelList
                await Task.Yield();

                var dailyViewsModelList = new List<DailyViewsModel>();

                for (int i = 1; i < 14; i++)
                {
                    var count = DemoDataConstants.GetRandomNumber();
                    var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

                    dailyViewsModelList.Add(new DailyViewsModel(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
                }

                //Add one trending repo
                dailyViewsModelList.Add(new DailyViewsModel(DateTimeOffset.UtcNow, DemoDataConstants.MaximumRandomNumber * 4, DemoDataConstants.MaximumRandomNumber / 2));

                return new RepositoryViewsResponseModel(dailyViewsModelList.Sum(x => x.TotalViews), dailyViewsModelList.Sum(x => x.TotalUniqueViews), dailyViewsModelList, repo, owner);
            }
            else
            {
                var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
                var response = await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryViewStatistics(owner, repo, GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

                return new RepositoryViewsResponseModel(response.TotalCount, response.TotalUniqueCount, response.DailyViewsList, repo, owner);
            }
        }

        public async Task<RepositoryClonesResponseModel> GetRepositoryCloneStatistics(string owner, string repo, CancellationToken cancellationToken)
        {
            if (_gitHubUserService.IsDemoUser)
            {
                //Yield off of the main thread to generate dailyViewsModelList
                await Task.Yield();

                var dailyViewsModelList = new List<DailyClonesModel>();

                for (int i = 0; i < 14; i++)
                {
                    var count = DemoDataConstants.GetRandomNumber() / 2; //Ensures the average clone count is smaller than the average view count
                    var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

                    dailyViewsModelList.Add(new DailyClonesModel(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
                }

                return new RepositoryClonesResponseModel(dailyViewsModelList.Sum(x => x.TotalClones), dailyViewsModelList.Sum(x => x.TotalUniqueClones), dailyViewsModelList, repo, owner);
            }
            else
            {
                var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
                var response = await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryCloneStatistics(owner, repo, GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

                return new RepositoryClonesResponseModel(response.TotalCount, response.TotalUniqueCount, response.DailyClonesList, repo, owner);
            }
        }

        public async Task<HttpResponseMessage> GetGitHubApiResponse(CancellationToken cancellationToken)
        {
            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            if (_gitHubUserService.IsDemoUser)
                return await AttemptAndRetry_Mobile(() => GithubApiClient.GetGitHubApiResponse_Unauthenticated(), cancellationToken).ConfigureAwait(false);

            return await AttemptAndRetry_Mobile(() => GithubApiClient.GetGitHubApiResponse_Authenticated(GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<ReferringSiteModel>> GetReferringSites(string owner, string repo, CancellationToken cancellationToken)
        {
            if (_gitHubUserService.IsDemoUser)
            {
                //Yield off of main thread to generate MobileReferringSiteModels
                await Task.Yield();

                var referringSitesList = new List<ReferringSiteModel>();

                for (int i = 0; i < DemoDataConstants.ReferringSitesCount; i++)
                {
                    referringSitesList.Add(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomText()));
                }

                return referringSitesList;
            }
            else
            {
                var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
                var referringSites = await AttemptAndRetry_Mobile(() => GithubApiClient.GetReferingSites(owner, repo, GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

                return referringSites;
            }
        }
    }
}
