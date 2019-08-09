using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public class AzureFunctionsApiService : BaseMobileApiService
    {
        readonly Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new Lazy<IAzureFunctionsApi>(() => RestService.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));

        IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;

        public Task<GetGitHubClientIdDTO> GetGitHubClientId() => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GetGitTrendsClientId());
        public Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO) => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO));
        public Task<SyncfusionDTO> GetSyncfusionInformation() => ExecuteMobilePollyFunction(() => AzureFunctionsApiClient.GetSyncfusionInformation(SyncfusionService.AssemblyVersionNumber));
    }
}
