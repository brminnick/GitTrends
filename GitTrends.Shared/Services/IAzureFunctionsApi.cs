using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("Accept-Encoding: gzip", "Accept: application/json")]
    interface IAzureFunctionsApi
    {
        [Get(@"/GetGitHubClientId")]
        Task<GetGitHubClientIdDTO> GetGitTrendsClientId();

        [Post(@"/GenerateGitHubOAuthToken")]
        Task<GitHubToken> GenerateGitTrendsOAuthToken([Body] GenerateTokenDTO generateTokenDTO);

        [Get(@"/GetSyncfusionInformation/{licenseVersion}")]
        Task<SyncfusionDTO> GetSyncfusionInformation(long licenseVersion, [AliasAs("code")] string functionKey = AzureConstants.GetSyncFusionInformationApiKey);
    }
}
