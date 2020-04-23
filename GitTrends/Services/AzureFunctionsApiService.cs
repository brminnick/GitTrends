using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public class AzureFunctionsApiService : BaseMobileApiService
    {
        readonly static Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new Lazy<IAzureFunctionsApi>(() => RestService.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));

        public AzureFunctionsApiService(AnalyticsService analyticsService) : base(analyticsService)
        {

        }

        static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;

        public Task<GetGitHubClientIdDTO> GetGitHubClientId(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetGitTrendsClientId(), cancellationToken);
        public Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO, CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO), cancellationToken);
        public Task<SyncFusionDTO> GetSyncfusionInformation(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetSyncfusionInformation(SyncFusionService.AssemblyVersionNumber), cancellationToken);
        public Task<GetChartVideoDTO> GetChartVideoUrl(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetChartVideoUrl(), cancellationToken);
    }
}
