using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials;

namespace GitTrends
{
    public static class SyncfusionService
    {
        readonly static Lazy<long> _assemblyVersionNumberHolder = new Lazy<long>(() => long.Parse(System.Reflection.Assembly.GetAssembly(typeof(Syncfusion.CoreAssembly)).GetName().Version.ToString().Replace(".", "")));
        readonly static Lazy<string> _syncfusionLicenseKeyHolder = new Lazy<string>(() => $"{nameof(SyncfusionDTO.LicenseKey)}_{AssemblyVersionNumber}");

        public static long AssemblyVersionNumber => _assemblyVersionNumberHolder.Value;

        public static async Task Initialize()
        {
            string syncFusionLicense;

            try
            {
                var syncusionDto = await AzureFunctionsApiService.GetSyncfusionInformation().ConfigureAwait(false);

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

        public static Task<string> GetLicense() => SecureStorage.GetAsync(_syncfusionLicenseKeyHolder.Value);

        static Task SaveLicense(string license) => SecureStorage.SetAsync(_syncfusionLicenseKeyHolder.Value, license);
    }
}
