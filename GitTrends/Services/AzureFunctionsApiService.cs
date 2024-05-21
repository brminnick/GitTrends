using GitTrends.Shared;

namespace GitTrends;

public class AzureFunctionsApiService(IAnalyticsService analyticsService,
										IAzureFunctionsApi azureFunctionsApi) : BaseMobileApiService(analyticsService)
{
	readonly IAzureFunctionsApi _azureFunctionsApiClient = azureFunctionsApi;

	public Task<GetGitHubClientIdDTO> GetGitHubClientId(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetGitTrendsClientId(cancellationToken);
	public Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO, CancellationToken cancellationToken) => _azureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO, cancellationToken);
	public Task<SyncFusionDTO> GetSyncfusionInformation(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetSyncfusionInformation(SyncfusionService.AssemblyVersionNumber, cancellationToken);
	public Task<NotificationHubInformation> GetNotificationHubInformation(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetNotificationHubInformation(cancellationToken);
	public Task<IReadOnlyList<NuGetPackageModel>> GetLibraries(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetLibraries(cancellationToken);
	public Task<GitTrendsStatisticsDTO> GetGitTrendsStatistics(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetGitTrendsStatistics(cancellationToken);
	public Task<AppCenterApiKeyDTO> GetAppCenterApiKeys(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetAppCenterApiKeys(cancellationToken);
	public Task<GitTrendsEnableOrganizationsUriDTO> GetGitTrendsEnableOrganizationsUri(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetGitTrendsEnableOrganizationsUri(cancellationToken);

	public async Task<IReadOnlyDictionary<string, StreamingManifest>> GetStreamingManifests(CancellationToken cancellationToken)
	{
		var streamingManifestDictionary = await _azureFunctionsApiClient.GetStreamingManifests(cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Deserialized value cannot be null");
		return new Dictionary<string, StreamingManifest>(streamingManifestDictionary);
	}
}