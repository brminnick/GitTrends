using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials;

namespace GitTrends
{
    public class SyncFusionService
    {
        readonly static Lazy<long> _assemblyVersionNumberHolder = new Lazy<long>(() => long.Parse(System.Reflection.Assembly.GetAssembly(typeof(Syncfusion.CoreAssembly)).GetName().Version.ToString().Replace(".", "")));
        readonly static Lazy<string> _syncfusionLicenseKeyHolder = new Lazy<string>(() => $"{nameof(SyncFusionDTO.LicenseKey)}{_assemblyVersionNumberHolder.Value}");

        readonly AzureFunctionsApiService _azureFunctionsApiService;
        readonly IAnalyticsService _analyticsService;

        public SyncFusionService(AzureFunctionsApiService azureFunctionsApiService, IAnalyticsService analyticsService) =>
           (_azureFunctionsApiService, _analyticsService) = (azureFunctionsApiService, analyticsService);

        public static long AssemblyVersionNumber => _assemblyVersionNumberHolder.Value;

        string SyncfusionLicenseKey => _syncfusionLicenseKeyHolder.Value;

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

            if (!string.IsNullOrWhiteSpace(syncFusionLicense))
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncFusionLicense);
            else
                throw new SyncFusionLicenseException($"{nameof(syncFusionLicense)} is empty");
        }

        public Task<string?> GetLicense() => SecureStorage.GetAsync(SyncfusionLicenseKey);

        Task SaveLicense(in string license) => SecureStorage.SetAsync(SyncfusionLicenseKey, license);

        class SyncFusionLicenseException : Exception
        {
            public SyncFusionLicenseException(string message) : base(message)
            {

            }
        }
    }
}
