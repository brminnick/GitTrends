using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials;

namespace GitTrends.Mobile.Shared
{
    public static class SyncfusionService
    {
        public static async Task Initialize()
        {
#if __IOS__
            Syncfusion.SfChart.XForms.iOS.Renderers.SfChartRenderer.Init();
#endif
            string syncFusionLicense;

            try
            {
                var syncFusionDto = await AzureFunctionsApiService.GetSyncfusionInformation().ConfigureAwait(false);

                syncFusionLicense = syncFusionDto.LicenseKey;

                await SaveSyncFusionLicense(syncFusionLicense).ConfigureAwait(false);
            }
            catch(System.Exception e)
            {
                syncFusionLicense = await GetSyncFusionLicense().ConfigureAwait(false);
                // https://gittrendsfunctions.azurewebsites.net/api/GetSyncfusionInformation/{licenseVersion:alpha}?code=4CGtOo2YByzUOCZ5TiZenJiXSIo3/KU8tKWHLRBlIlxYvrcg7clr5g==

                // HttpRequest Uri {https://gittrendsfunctions.azurewebsites.net/api/GetSyncfusionInformation/17_1_0_50?code=4CGtOo2YByzUOCZ5TiZenJiXSIo3%2FKU8tKWHLRBlIlxYvrcg7clr5g%3D%3D}
            }

            if (!string.IsNullOrWhiteSpace(syncFusionLicense))
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncFusionLicense);
        }

        public static Task<string> GetSyncFusionLicense() => SecureStorage.GetAsync(nameof(SyncfusionDTO.LicenseKey));

        static Task SaveSyncFusionLicense(string license) => SecureStorage.SetAsync(nameof(SyncfusionDTO.LicenseKey), license);
    }
}
