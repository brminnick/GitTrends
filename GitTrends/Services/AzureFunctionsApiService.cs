using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class AzureFunctionsApiService : BaseMobileApiService
    {
        readonly static Lazy<IAzureFunctionsApi> _azureFunctionsApiClientHolder = new Lazy<IAzureFunctionsApi>(() => RestService.For<IAzureFunctionsApi>(CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)));

        public AzureFunctionsApiService(IAnalyticsService analyticsService, IMainThread mainThread) : base(analyticsService, mainThread)
        {

        }

        static IAzureFunctionsApi AzureFunctionsApiClient => _azureFunctionsApiClientHolder.Value;

        public Task<GetGitHubClientIdDTO> GetGitHubClientId(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetGitTrendsClientId(), cancellationToken);
        public Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO, CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO), cancellationToken);
        public Task<SyncFusionDTO> GetSyncfusionInformation(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetSyncfusionInformation(SyncfusionService.AssemblyVersionNumber), cancellationToken);
        public Task<StreamingManifest> GetChartStreamingUrl(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetChartStreamingUrl(), cancellationToken);
        public Task<NotificationHubInformation> GetNotificationHubInformation(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => AzureFunctionsApiClient.GetNotificationHubInformation(), cancellationToken);
    }
}
