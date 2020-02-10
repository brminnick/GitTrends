using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public class GitHubApiV3Service : BaseMobileApiService
    {
        readonly Lazy<IGitHubApiV3> _githubApiClient = new Lazy<IGitHubApiV3>(() => RestService.For<IGitHubApiV3>(CreateHttpClient(GitHubConstants.GitHubRestApiUrl)));

        IGitHubApiV3 GithubApiClient => _githubApiClient.Value;

        public async Task<RepositoryViewsModel> GetRepositoryViewStatistics(string owner, string repo)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            return await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryViewStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
        }

        public async Task<RepositoryClonesModel> GetRepositoryCloneStatistics(string owner, string repo)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            return await AttemptAndRetry_Mobile(() => GithubApiClient.GetRepositoryCloneStatistics(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<MobileReferringSiteModel> GetReferingSites(string owner, string repo)
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
