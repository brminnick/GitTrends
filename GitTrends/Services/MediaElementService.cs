using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Newtonsoft.Json;

namespace GitTrends
{
	public class MediaElementService(IDeviceInfo deviceInfo,
										IPreferences preferences,
										IAnalyticsService analyticsService,
										AzureFunctionsApiService azureFunctionsApiService)
	{
		static readonly WeakEventManager<StreamingManifest?> _streamingManifestChangedEventManager = new();

		readonly IDeviceInfo _deviceInfo = deviceInfo;
		readonly IPreferences _preferences = preferences;
		readonly IAnalyticsService _analyticsService = analyticsService;
		readonly AzureFunctionsApiService _azureFunctionsApiService = azureFunctionsApiService;

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

			if (_deviceInfo.Platform == DevicePlatform.Android)
				return OnboardingChartManifest.DashUrl;
			else if (_deviceInfo.Platform == DevicePlatform.iOS)
				return OnboardingChartManifest.HlsUrl;
			else
				throw new NotSupportedException("Platform Not Supported");
		}

		string GetEnableOrganizationsUrl()
		{
			if (EnableOrganizationsManifest is null)
				throw new ArgumentNullException(nameof(EnableOrganizationsManifest));

			if (_deviceInfo.Platform == DevicePlatform.Android)
				return EnableOrganizationsManifest.DashUrl;
			else if (_deviceInfo.Platform == DevicePlatform.iOS)
				return EnableOrganizationsManifest.HlsUrl;
			else
				throw new NotSupportedException("Platform Not Supported");
		}

		StreamingManifest? GetStreamingManifest([CallerMemberName] string key = "")
		{
			try
			{
				var serializedManifest = _preferences.Get<string?>(key, null);
				if (serializedManifest is null)
					return null;

				return JsonConvert.DeserializeObject<StreamingManifest>(serializedManifest);
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