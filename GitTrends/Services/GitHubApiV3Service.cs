using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubApiV3Service : BaseMobileApiService
    {
        readonly IGitHubApiV3 _githubApiClient;
        readonly GitHubUserService _gitHubUserService;

        public GitHubApiV3Service(IMainThread mainThread,
                                    IGitHubApiV3 gitHubApiV3,
                                    IAnalyticsService analyticsService,
                                    GitHubUserService gitHubUserService) : base(analyticsService, mainThread)
        {
            _githubApiClient = gitHubApiV3;
            _gitHubUserService = gitHubUserService;
        }

        public Task<RepositoryFile> GetGitTrendsFile(string fileName, CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _githubApiClient.GetGitTrendsFile(fileName), cancellationToken);

        public async Task<RepositoryViewsResponseModel> GetRepositoryViewStatistics(string owner, string repo, CancellationToken cancellationToken)
        {
            if (_gitHubUserService.IsDemoUser)
            {
                //Yield off of the main thread to generate dailyViewsModelList
                await Task.Yield();

                var dailyViewsModelList = new List<DailyViewsModel>();

                for (int i = 0; i < 14; i++)
                {
                    var count = DemoDataConstants.GetRandomNumber();
                    var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

                    //Ensures one Demo repo is Trending
                    if (i is 13 && new Random().Next(0, DemoDataConstants.RepoCount) is DemoDataConstants.RepoCount - 1 or DemoDataConstants.RepoCount - 2)
                        dailyViewsModelList.Add(new DailyViewsModel(DateTimeOffset.UtcNow, DemoDataConstants.MaximumRandomNumber * 4, DemoDataConstants.MaximumRandomNumber / 2));
                    else
                        dailyViewsModelList.Add(new DailyViewsModel(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
                }

                return new RepositoryViewsResponseModel(dailyViewsModelList.Sum(x => x.TotalViews), dailyViewsModelList.Sum(x => x.TotalUniqueViews), dailyViewsModelList, repo, owner);
            }
            else
            {
                var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
                var response = await AttemptAndRetry_Mobile(() => _githubApiClient.GetRepositoryViewStatistics(owner, repo, GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

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
                var response = await AttemptAndRetry_Mobile(() => _githubApiClient.GetRepositoryCloneStatistics(owner, repo, GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

                return new RepositoryClonesResponseModel(response.TotalCount, response.TotalUniqueCount, response.DailyClonesList, repo, owner);
            }
        }

        public async Task<HttpResponseMessage> GetGitHubApiResponse(CancellationToken cancellationToken)
        {
            var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

            if (!_gitHubUserService.IsAuthenticated)
                return await AttemptAndRetry_Mobile(() => _githubApiClient.GetGitHubApiResponse_Unauthenticated(), cancellationToken).ConfigureAwait(false);

            return await AttemptAndRetry_Mobile(() => _githubApiClient.GetGitHubApiResponse_Authenticated(GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);
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
                var referringSites = await AttemptAndRetry_Mobile(() => _githubApiClient.GetReferingSites(owner, repo, GetGitHubBearerTokenHeader(token)), cancellationToken).ConfigureAwait(false);

                return referringSites;
            }
        }
    }
}
