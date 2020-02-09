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

        public async Task<List<ReferringSiteModel>> GetReferingSites(string owner, string repo)
        {
            var token = await GitHubAuthenticationService.GetGitHubToken().ConfigureAwait(false);
            var referringSites = await AttemptAndRetry_Mobile(() => GithubApiClient.GetReferingSites(owner, repo, GetGitHubBearerTokenHeader(token))).ConfigureAwait(false);

            await setFavIcons(referringSites).ConfigureAwait(false);

            return referringSites;

            static Task setFavIcons(IEnumerable<ReferringSiteModel> referringSites)
            {
                return Task.WhenAll(referringSites.Select(x => setFavIcon(x)));

                static async Task setFavIcon(ReferringSiteModel referringSiteModel)
                {
                    if (referringSiteModel.ReferrerUri != null)
                    {
                        var favIcon = await FavIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri.ToString()).ConfigureAwait(false);
                        referringSiteModel.FavIcon = favIcon;
                    }
                }
            }
        }
    }
}
