using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("User-Agent: " + nameof(GitTrends), "Accept-Encoding: gzip", "Accept: application/json")]
    interface IGitHubApiV3
    {
        [Get("/repos/{owner}/{repo}/traffic/views")]
        Task<RepositoryViewsModel> GetRepositoryViewStatistics(string owner, string repo, [Header("Authorization")] string authorization);

        [Get("/repos/{owner}/{repo}/traffic/clones")]
        Task<RepositoryClonesModel> GetRepositoryCloneStatistics(string owner, string repo, [Header("Authorization")] string authorization);
    }
}
