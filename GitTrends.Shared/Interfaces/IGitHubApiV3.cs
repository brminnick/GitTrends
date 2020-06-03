using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("User-Agent: " + nameof(GitTrends), "Accept-Encoding: gzip", "Accept: application/json")]
    interface IGitHubApiV3
    {
        [Get("/repos/{owner}/{repo}/traffic/views")]
        Task<RepositoryViewsResponseModel> GetRepositoryViewStatistics(string owner, string repo, [Header("Authorization")] string authorization);

        [Get("/repos/{owner}/{repo}/traffic/clones")]
        Task<RepositoryClonesResponseModel> GetRepositoryCloneStatistics(string owner, string repo, [Header("Authorization")] string authorization);

        [Get("/repos/{owner}/{repo}/traffic/popular/referrers")]
        Task<List<ReferringSiteModel>> GetReferingSites(string owner, string repo, [Header("Authorization")] string authorization);

        [Get("/repos/xamarin/xamarin.forms")]
        Task<HttpResponseMessage> GetGitHubApiResponse_Authenticated([Header("Authorization")] string authorization);

        [Get("/repos/xamarin/xamarin.forms")]
        Task<HttpResponseMessage> GetGitHubApiResponse_Unauthenticated();
    }
}
