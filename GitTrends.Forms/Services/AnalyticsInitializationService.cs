using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public class AnalyticsInitializationService
	{
		const string _iOSDebugKey = "0e194e2a-3aad-41c5-a6bc-61900e185075";
		const string _androidDebugKey = "272973ed-d3cc-4ee0-b4f2-5b5d01ad487d";

		readonly IDeviceInfo _deviceInfo;
		readonly IPreferences _preferences;
		readonly IAnalyticsService _analyticsService;
		readonly AzureFunctionsApiService _azureFunctionsApiService;

		public AnalyticsInitializationService(IDeviceInfo deviceInfo,
												IPreferences preferences,
												IAnalyticsService analyticsService,
												AzureFunctionsApiService azureFunctionsApiService)
		{
			_deviceInfo = deviceInfo;
			_preferences = preferences;
			_analyticsService = analyticsService;
			_azureFunctionsApiService = azureFunctionsApiService;

			if (!string.IsNullOrWhiteSpace(ApiKey))
				_analyticsService.Start(ApiKey);
		}

		string ApiKey
		{
			get
			{
#if AppStore
                if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                    return iOSApiKey;

                if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
                    return AndroidApiKey;
#else
				if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
					return _iOSDebugKey;

				if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
					return _androidDebugKey;
#endif
				throw new NotSupportedException();
			}
		}

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
				await initialize().ConfigureAwait(false);
			else
				initialize().SafeFireAndForget(ex => _analyticsService.Report(ex));

			async Task initialize()
			{
				var appCenterApiKeys = await _azureFunctionsApiService.GetAppCenterApiKeys(cancellationToken).ConfigureAwait(false);

				AndroidApiKey = appCenterApiKeys.Android;
				iOSApiKey = appCenterApiKeys.iOS;

				var isConfigured = _analyticsService.Configured;

				if (!isConfigured)
					_analyticsService.Start(ApiKey);
			}
		}
	}
}