using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class AnalyticsInitializationService
    {
        const string _iOSDebugKey = "0e194e2a-3aad-41c5-a6bc-61900e185075";
        const string _androidDebugKey = "272973ed-d3cc-4ee0-b4f2-5b5d01ad487d";

        readonly IPreferences _preferences;
        readonly AzureFunctionsApiService _azureFunctionsApiService;

        public AnalyticsInitializationService(IPreferences preferences, AzureFunctionsApiService azureFunctionsApiService)
        {
            _preferences = preferences;
            _azureFunctionsApiService = azureFunctionsApiService;

            if (!string.IsNullOrWhiteSpace(ApiKey))
                AppCenter.Start(ApiKey, typeof(Analytics), typeof(Crashes));
        }

        string ApiKey => Xamarin.Forms.Device.RuntimePlatform switch
        {
#if AppStore

            Xamarin.Forms.Device.iOS => iOSApiKey,
            Xamarin.Forms.Device.Android => AndroidApiKey,
#else
            Xamarin.Forms.Device.iOS => _iOSDebugKey,
            Xamarin.Forms.Device.Android => _androidDebugKey,
#endif
            _ => throw new NotSupportedException()
        };

        string AndroidApiKey
        {
            get => _preferences.Get(nameof(AndroidApiKey), string.Empty);
            set => _preferences.Set(nameof(AndroidApiKey), value);
        }

#pragma warning disable IDE1006 // Naming Styles
        string iOSApiKey
        {
            get => _preferences.Get(nameof(iOSApiKey), string.Empty);
            set => _preferences.Set(nameof(iOSApiKey), value);
        }
#pragma warning restore IDE1006 // Naming Styles

        public async ValueTask Initialize(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                var appCenterApiKeys = await _azureFunctionsApiService.GetAppCenterApiKeys(cancellationToken).ConfigureAwait(false);

                AndroidApiKey = appCenterApiKeys.Android;
                iOSApiKey = appCenterApiKeys.iOS;
            }

            var isConfigured = AppCenter.Configured;

            if (!isConfigured)
                AppCenter.Start(ApiKey, typeof(Analytics), typeof(Crashes));
        }
    }
}
