using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public class GitHubApiV3Service : BaseMobileApiService
    {
        readonly static Lazy<IGitHubApiV3> _githubApiClient = new Lazy<IGitHubApiV3>(() => RestService.For<IGitHubApiV3>(CreateHttpClient(GitHubConstants.GitHubRestApiUrl)));

        static IGitHubApiV3 GithubApiClient => _githubApiClient.Value;

        public async Task<RepositoryViewsModel> GetRepositoryViewStatistics(string owner, string repo)
        {
            if (GitHubAuthenticationService.IsDemoUser)
            {
                //Yield off of the main thread to generate dailyViewsModelList
                await Task.Yield();

                var dailyViewsModelList = new List<DailyViewsModel>();

                for (int i = 0; i < 14; i++)
                {
                    var count = DemoDataConstants.GetRandomNumber();
                    var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

                    dailyViewsModelList.Add(new DailyViewsModel(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14 - i)), count, uniqeCount));
                }

                return new RepositoryViewsModel(dailyViewsModelList.Sum(x => x.TotalViews), dailyViewsModelList.Sum(x => x.TotalUniqueViews), dailyViewsModelList);
            }
            else
            {
                var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
                return await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryViewStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
            }
        }

        public async Task<RepositoryClonesModel> GetRepositoryCloneStatistics(string owner, string repo)
        {
            if (GitHubAuthenticationService.IsDemoUser)
            {
                //Yield off of the main thread to generate dailyViewsModelList
                await Task.Yield();

                var dailyViewsModelList = new List<DailyClonesModel>();

                for (int i = 0; i < 14; i++)
                {
                    var count = DemoDataConstants.GetRandomNumber() / 2; //Ensures the average clone count is smaller than the average view count
                    var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

                    dailyViewsModelList.Add(new DailyClonesModel(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14 - i)), count, uniqeCount));
                }

                return new RepositoryClonesModel(dailyViewsModelList.Sum(x => x.TotalClones), dailyViewsModelList.Sum(x => x.TotalUniqueClones), dailyViewsModelList);
            }
            else
            {
                var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
                return await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryCloneStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
            }
        }

        public async IAsyncEnumerable<MobileReferringSiteModel> GetReferingSites(string owner, string repo)
        {
            if (GitHubAuthenticationService.IsDemoUser)
            {
                //Yield off of main thread to generate MobileReferringSiteModels
                await Task.Yield();

                for (int i = 0; i < DemoDataConstants.ReferringSitesCount; i++)
                    yield return new MobileReferringSiteModel(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomText()), "DefaultProfileImage");

                //Allow UI to update
                await Task.Delay(1000).ConfigureAwait(false);
            }
            else
            {
                var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
                var referringSites = await AttemptAndRetry_Mobile(() => GithubApiClient.GetReferingSites(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);

                await foreach (var referringSite in getMobileReferringSiteModels(referringSites).ConfigureAwait(false))
                {
                    yield return referringSite;
                }

                static async IAsyncEnumerable<MobileReferringSiteModel> getMobileReferringSiteModels(IEnumerable<ReferringSiteModel> referringSites)
                {
                    var setFavIconTaskList = referringSites.Select(x => setFavIcon(x)).ToList();

                    while (setFavIconTaskList.Any())
                    {
                        var completedSetFavIconTask = await Task.WhenAny(setFavIconTaskList).ConfigureAwait(false);

                        setFavIconTaskList.Remove(completedSetFavIconTask);

                        yield return await completedSetFavIconTask.ConfigureAwait(false);
                    }

                    static async Task<MobileReferringSiteModel> setFavIcon(ReferringSiteModel referringSiteModel)
                    {
                        if (referringSiteModel.ReferrerUri != null)
                        {
                            var favIcon = await FavIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri.ToString()).ConfigureAwait(false);
                            return new MobileReferringSiteModel(referringSiteModel, favIcon);
                        }

                        return new MobileReferringSiteModel(referringSiteModel, null);
                    }
                }
            }
        }
    }
}
