using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public abstract class AzureFunctionsApiService : BaseMobileApiService
    {
        static readonly Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new Lazy<IAzureFunctionsApi>(() => RestService.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));

        static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;

        public static Task<GetGitHubClientIdDTO> GetGitHubClientId() => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GetGitTrendsClientId());
        public static Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO) => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO));
        public static Task<SyncfusionDTO> GetSyncfusionInformation() => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GetSyncfusionInformation(SyncfusionService.AssemblyVersionNumber));
    }
}
