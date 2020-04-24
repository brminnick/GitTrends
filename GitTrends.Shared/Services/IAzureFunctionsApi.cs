using System.Threading.Tasks;
using Refit;

namespace GitTrends.Shared
{
    [Headers("Accept-Encoding: gzip", "Accept: application/json")]
    interface IAzureFunctionsApi
    {
        [Get("/GetGitHubClientId")]
        Task<GetGitHubClientIdDTO> GetGitTrendsClientId();

        [Get("/GenerateGitHubOAuthToken")]
        Task<GitHubToken> GenerateGitTrendsOAuthToken([Body] GenerateTokenDTO generateTokenDTO);

        [Get("/GetSyncfusionInformation")]
        Task<SyncFusionDTO> GetSyncfusionInformation(long licenseVersion, [AliasAs("code")] string functionKey = AzureConstants.GetSyncFusionInformationApiKey);

        [Get("/GetUITestToken")]
        Task<GitHubToken> GetUITestToken([AliasAs("code")] string functionKey = AzureConstants.GetUITestTokenApiKey);
    }
}
