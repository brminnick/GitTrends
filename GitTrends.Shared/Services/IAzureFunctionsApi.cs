using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("Accept-Encoding: gzip", "Accept: application/json")]
    interface IAzureFunctionsApi
    {
        [Get(@"/GetGitHubClientId")]
        Task<GetGitHubClientIdDTO> GetGitTrendsClientId([AliasAs("code")] string functionKey = AzureConstants.GetGitTrendsClientIdApiKey);

        [Post(@"/GenerateGitHubOAuthToken")]
        Task<GitHubToken> GenerateGitTrendsOAuthToken([Body] GenerateTokenDTO generateTokenDTO, [AliasAs("code")] string functionKey = AzureConstants.GenerateGitTrendsOAuthTokenApiKey);

        [Get(@"/GetSyncFusionInformation")]
        Task<SyncFusionDTO> GetSyncFusionInformation(string licenseVersion, [AliasAs("code")] string functionKey = AzureConstants.GetSyncFusionInformationApiKey);
    }
}
