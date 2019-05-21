using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("Accept-Encoding: gzip", "Accept: application/json")]
    interface IAzureFunctionsApi
    {
        [Get(@"/GetGitHubClientId")]
        Task<string> GetGitTrendsClientId([AliasAs("code")] string functionKey);

        [Post(@"/GenerateGitHubOAuthToken")]
        Task<GitHubToken> GenerateGitTrendsOAuthToken([Body] GenerateTokenDTO generateTokenDTO, [AliasAs("code")] string functionKey);
    }
}
