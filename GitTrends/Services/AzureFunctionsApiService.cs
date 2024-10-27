using GitTrends.Common;

namespace GitTrends;

public class AzureFunctionsApiService(IAnalyticsService analyticsService,
										IAzureFunctionsApi azureFunctionsApi) : BaseMobileApiService(analyticsService)
{
	readonly IAzureFunctionsApi _azureFunctionsApiClient = azureFunctionsApi;

	public Task<GetGitHubClientIdDTO> GetGitHubClientId(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetGitTrendsClientId(cancellationToken);
	public Task<GitHubToken> GenerateGitTrendsOAuthToken(GenerateTokenDTO generateTokenDTO, CancellationToken cancellationToken) => _azureFunctionsApiClient.GenerateGitTrendsOAuthToken(generateTokenDTO, cancellationToken);
	public Task<SyncFusionDTO> GetSyncfusionInformation(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetSyncfusionInformation(SyncfusionService.AssemblyVersionNumber, cancellationToken);
	public Task<IReadOnlyList<NuGetPackageModel>> GetLibraries(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetLibraries(cancellationToken);
	public Task<GitTrendsStatisticsDTO> GetGitTrendsStatistics(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetGitTrendsStatistics(cancellationToken);
	public Task<GitTrendsEnableOrganizationsUriDTO> GetGitTrendsEnableOrganizationsUri(CancellationToken cancellationToken) => _azureFunctionsApiClient.GetGitTrendsEnableOrganizationsUri(cancellationToken);
}