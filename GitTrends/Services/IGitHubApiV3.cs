using Refit;
using System.Threading.Tasks;

namespace GitTrends
{
    [Headers("User-Agent: " + nameof(GitTrends))]
    interface IGitHubApiV3
    {
        [Get("repos/{owner}/{repo}/traffic/views")]
        Task<RepositoryViewsModel> GetRepositoryViewStatistics(string owner, string repo);

        [Get("repos/{owner}/{repo}/traffic/clones")]
        Task<RepositoryClonesModel> GetRepositoryCloneStatistics(string owner, string repo);
    }
}
