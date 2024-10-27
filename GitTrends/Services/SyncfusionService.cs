using GitTrends.Common;

namespace GitTrends;

public class SyncfusionService(ISecureStorage secureStorage,
								IAnalyticsService analyticsService,
								AzureFunctionsApiService azureFunctionsApiService)
{
	static readonly Lazy<long> _assemblyVersionNumberHolder = new(() => long.Parse(System.Reflection.Assembly.GetAssembly(typeof(Syncfusion.CoreAssembly))?.GetName().Version?.ToString()?.Replace(".", "")
																			?? throw new InvalidOperationException("Syncfusion Assembly Version Number Not Found")));
	static readonly Lazy<string> _syncfusionLicenseKeyHolder = new(() => $"{nameof(SyncFusionDTO.LicenseKey)}{_assemblyVersionNumberHolder.Value}");

	readonly ISecureStorage _secureStorage = secureStorage;
	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly AzureFunctionsApiService _azureFunctionsApiService = azureFunctionsApiService;

	public static long AssemblyVersionNumber => _assemblyVersionNumberHolder.Value;

	static string SyncfusionLicenseKey => _syncfusionLicenseKeyHolder.Value;

	public async Task Initialize(CancellationToken cancellationToken)
	{
		var syncFusionLicense = await GetLicense().ConfigureAwait(false);

		if (string.IsNullOrWhiteSpace(syncFusionLicense))
		{
			try
			{
				var syncusionDto = await _azureFunctionsApiService.GetSyncfusionInformation(cancellationToken).ConfigureAwait(false);

				syncFusionLicense = syncusionDto.LicenseKey;

				await SaveLicense(syncFusionLicense).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_analyticsService.Report(e);
			}
		}

		if (string.IsNullOrWhiteSpace(syncFusionLicense))
			throw new SyncFusionLicenseException($"{nameof(syncFusionLicense)} is empty");
		else
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncFusionLicense);
	}

	public Task<string?> GetLicense() => _secureStorage.GetAsync(SyncfusionLicenseKey);

	Task SaveLicense(in string license) => _secureStorage.SetAsync(SyncfusionLicenseKey, license);

	sealed class SyncFusionLicenseException(string message) : Exception(message)
	{
	}
}