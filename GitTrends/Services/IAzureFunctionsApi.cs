using System.Threading.Tasks;
using Octokit;
using Refit;

namespace GitTrends
{
    interface IAzureFunctionsApi
    {
        [Get(@"/GetGitHubClientId")]
        Task<string> GetGitTrendsClientId([AliasAs("code")] string functionKey);

        [Post(@"/GenerateGitHubOAuthToken")]
        Task<string> GenerateGitTrendsOAuthToken([Body]string loginCode, [AliasAs("code")] string functionKey);
    }
}
