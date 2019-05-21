using System.Threading.Tasks;
using GitTrends.Shared;
using Octokit;
using Refit;

namespace GitTrends
{
    interface IAzureFunctionsApi
    {
        [Get(@"/GetGitHubClientId")]
        Task<string> GetGitTrendsClientId([AliasAs("code")] string functionKey);

        [Get(@"/GenerateGitHubOAuthToken")]
        Task<OauthToken> GenerateGitTrendsOAuthToken([Body]GitHubOAuthModel gitHubOAuthModel, [AliasAs("code")] string functionKey);
    }
}
