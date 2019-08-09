using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials;

namespace GitTrends
{
    public class SyncfusionService
    {
        readonly static Lazy<long> _assemblyVersionNumberHolder = new Lazy<long>(() => long.Parse(System.Reflection.Assembly.GetAssembly(typeof(Syncfusion.CoreAssembly)).GetName().Version.ToString().Replace(".", "")));
        readonly Lazy<string> _syncfusionLicenseKeyHolder = new Lazy<string>(() => $"{nameof(SyncfusionDTO.LicenseKey)}_{_assemblyVersionNumberHolder.Value}");

        readonly AzureFunctionsApiService _azureFunctionsApiService;

        public SyncfusionService(AzureFunctionsApiService azureFunctionsApiService)
        {
            _azureFunctionsApiService = azureFunctionsApiService;
        }

        public static long AssemblyVersionNumber => _assemblyVersionNumberHolder.Value;
        public string SyncfusionLicenseKeyHolder => _syncfusionLicenseKeyHolder.Value;

        public async Task Initialize()
        {
            string syncFusionLicense;

            try
            {
                var syncusionDto = await _azureFunctionsApiService.GetSyncfusionInformation().ConfigureAwait(false);

                syncFusionLicense = syncusionDto.LicenseKey;

                await SaveLicense(syncFusionLicense).ConfigureAwait(false);
            }
            catch
            {
                syncFusionLicense = await GetLicense().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(syncFusionLicense))
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncFusionLicense);
        }

        public Task<string> GetLicense() => SecureStorage.GetAsync(SyncfusionLicenseKeyHolder);

        Task SaveLicense(string license) => SecureStorage.SetAsync(SyncfusionLicenseKeyHolder, license);
    }
}
