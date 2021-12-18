using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends;

public class AzureFunctionsApiService : BaseMobileApiService
{
	readonly IAzureFunctionsApi _azureFunctionsApiClient;

	public AzureFunctionsApiService(IMainThread mainThread,
									IAnalyticsService analyticsService,
									IAzureFunctionsApi azureFunctionsApi) : base(analyticsService, mainThread)
	{
		_azureFunctionsApiClient = azureFunctionsApi;
	}

	public Task<GetGitHubClientIdDTO> GetGitHubClientId(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetGitTrendsClientId(), cancellationToken);
	public Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO, CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO), cancellationToken);
	public Task<SyncFusionDTO> GetSyncfusionInformation(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetSyncfusionInformation(SyncfusionService.AssemblyVersionNumber), cancellationToken);
	public Task<NotificationHubInformation> GetNotificationHubInformation(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetNotificationHubInformation(), cancellationToken);
	public Task<IReadOnlyList<NuGetPackageModel>> GetLibraries(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetLibraries(), cancellationToken);
	public Task<GitTrendsStatisticsDTO> GetGitTrendsStatistics(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetGitTrendsStatistics(), cancellationToken);
	public Task<AppCenterApiKeyDTO> GetAppCenterApiKeys(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetAppCenterApiKeys(), cancellationToken);
	public Task<GitTrendsEnableOrganizationsUriDTO> GetGitTrendsEnableOrganizationsUri(CancellationToken cancellationToken) => AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetGitTrendsEnableOrganizationsUri(), cancellationToken);

	public async Task<IReadOnlyDictionary<string, StreamingManifest>> GetStreamingManifests(CancellationToken cancellationToken)
	{
		var streamingManifestDictionary = await AttemptAndRetry_Mobile(() => _azureFunctionsApiClient.GetStreamingManifests(), cancellationToken).ConfigureAwait(false) ?? throw new Newtonsoft.Json.JsonException();
		return new Dictionary<string, StreamingManifest>(streamingManifestDictionary);
	}
}