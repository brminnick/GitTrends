using Refit;

namespace  GitTrends.Common;

[Headers("Accept-Encoding: gzip", "Accept: application/json")]
public interface IAzureFunctionsApi
{
	[Get("/GetGitHubClientId")]
	Task<GetGitHubClientIdDTO> GetGitTrendsClientId(CancellationToken token);

	[Post("/GenerateGitHubOAuthToken")]
	Task<GitHubToken> GenerateGitTrendsOAuthToken([Body(true)] GenerateTokenDTO generateTokenDTO, CancellationToken token);

	[Get("/GetSyncfusionInformation/{licenseVersion}")]
	Task<SyncFusionDTO> GetSyncfusionInformation(long licenseVersion, CancellationToken token, [AliasAs("code")] string functionKey = AzureConstants.GetSyncFusionInformationApiKey);

	[Get("/GetTestToken")]
	Task<GitHubToken> GetTestToken(CancellationToken token, [AliasAs("code")] string functionKey = AzureConstants.GetTestTokenApiKey);

	[Get("/GetLibraries")]
	Task<IReadOnlyList<NuGetPackageModel>> GetLibraries(CancellationToken token);

	[Get("/GetGitTrendsStatistics")]
	Task<GitTrendsStatisticsDTO> GetGitTrendsStatistics(CancellationToken token);

	[Get("/GetGitTrendsEnableOrganizationsUri")]
	Task<GitTrendsEnableOrganizationsUriDTO> GetGitTrendsEnableOrganizationsUri(CancellationToken token);
}