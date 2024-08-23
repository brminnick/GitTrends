using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;

using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public class MediaElementService
	{
		static readonly WeakEventManager<StreamingManifest?> _streamingManifestChangedEventManager = new();

		readonly IDeviceInfo _deviceInfo;
		readonly IPreferences _preferences;
		readonly IAnalyticsService _analyticsService;
		readonly AzureFunctionsApiService _azureFunctionsApiService;

		public MediaElementService(IDeviceInfo deviceInfo,
									IPreferences preferences,
									IAnalyticsService analyticsService,
									AzureFunctionsApiService azureFunctionsApiService)
		{
			_deviceInfo = deviceInfo;
			_preferences = preferences;
			_analyticsService = analyticsService;
			_azureFunctionsApiService = azureFunctionsApiService;
		}

		public static event EventHandler<StreamingManifest?> OnboardingChartManifestChanged
		{
			add => _streamingManifestChangedEventManager.AddEventHandler(value);
			remove => _streamingManifestChangedEventManager.RemoveEventHandler(value);
		}

		public static event EventHandler<StreamingManifest?> EnableOrganizationsManifestChanged
		{
			add => _streamingManifestChangedEventManager.AddEventHandler(value);
			remove => _streamingManifestChangedEventManager.RemoveEventHandler(value);
		}

		public string OnboardingChartUrl => GetOnboardingChartUrl();

		public string EnableOrganizationsUrl => GetEnableOrganizationsUrl();

		public StreamingManifest? OnboardingChartManifest
		{
			get => GetStreamingManifest();
			private set
			{
				if (value != OnboardingChartManifest)
				{
					SetStreamingManifest(value);
					OnOnboardingChartStreamingManifestChanged(value);
				}
			}
		}

		public StreamingManifest? EnableOrganizationsManifest
		{
			get => GetStreamingManifest();
			private set
			{
				if (value != EnableOrganizationsManifest)
				{
					SetStreamingManifest(value);
					OnEnableOrganizationsStreamingManifestChanged(value);
				}
			}
		}

		public async ValueTask InitializeManifests(CancellationToken cancellationToken)
		{
			if (OnboardingChartManifest is null || EnableOrganizationsManifest is null)
				await initializeOnboardingChart(cancellationToken).ConfigureAwait(false);
			else
				initializeOnboardingChart(cancellationToken).SafeFireAndForget(ex => _analyticsService.Report(ex));

			async Task initializeOnboardingChart(CancellationToken cancellationToken)
			{
				var streamingManifests = await _azureFunctionsApiService.GetStreamingManifests(cancellationToken).ConfigureAwait(false);

				OnboardingChartManifest = streamingManifests[StreamingConstants.Chart];
				EnableOrganizationsManifest = streamingManifests[StreamingConstants.EnableOrganizations];
			}
		}

		string GetOnboardingChartUrl()
		{
			if (OnboardingChartManifest is null)
				throw new ArgumentNullException(nameof(OnboardingChartManifest));

			if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
				return OnboardingChartManifest.DashUrl;
			else if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
				return OnboardingChartManifest.HlsUrl;
			else
				throw new NotSupportedException("Platform Not Supported");
		}

		string GetEnableOrganizationsUrl()
		{
			if (EnableOrganizationsManifest is null)
				throw new ArgumentNullException(nameof(EnableOrganizationsManifest));

			if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
				return EnableOrganizationsManifest.DashUrl;
			else if (_deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
				return EnableOrganizationsManifest.HlsUrl;
			else
				throw new NotSupportedException("Platform Not Supported");
		}

		StreamingManifest? GetStreamingManifest([CallerMemberName] string key = "")
		{
			try
			{
				return JsonConvert.DeserializeObject<StreamingManifest?>(_preferences.Get(key, null));
			}
			catch (ArgumentNullException)
			{
				return null;
			}
			catch (JsonReaderException)
			{
				return null;
			}
		}

		void SetStreamingManifest(StreamingManifest? streamingManifest, [CallerMemberName] string key = "") => _preferences.Set(key, JsonConvert.SerializeObject(streamingManifest));

		void OnOnboardingChartStreamingManifestChanged(StreamingManifest? onboardingChartStreamingManifest) => _streamingManifestChangedEventManager.RaiseEvent(this, onboardingChartStreamingManifest, nameof(OnboardingChartManifestChanged));
		void OnEnableOrganizationsStreamingManifestChanged(StreamingManifest? enableOrganizationsStreamingManifest) => _streamingManifestChangedEventManager.RaiseEvent(this, enableOrganizationsStreamingManifest, nameof(EnableOrganizationsManifestChanged));
	}
}