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

        public Task<GetGitHubClientIdDTO> GetGitHubClientId() => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetGitTrendsClientId());
        public Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO));
        public Task<SyncfusionDTO> GetSyncfusionInformation() => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetSyncfusionInformation(SyncfusionService.AssemblyVersionNumber));
    }
}
