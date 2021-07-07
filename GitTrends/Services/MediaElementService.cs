using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class MediaElementService
    {
        readonly static WeakEventManager<StreamingManifest?> _streamingManifestChangedEventManager = new();

        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;
        readonly AzureFunctionsApiService _azureFunctionsApiService;

        public MediaElementService(IPreferences preferences,
                                    IAnalyticsService analyticsService,
                                    AzureFunctionsApiService azureFunctionsApiService)
        {
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

        public async ValueTask InitializeOnboardingChart(CancellationToken cancellationToken)
        {
            if (OnboardingChartManifest is null || EnableOrganizationsManifest is null)
                await initializeOnboardingChart(cancellationToken).ConfigureAwait(false);

            initializeOnboardingChart(cancellationToken).SafeFireAndForget();

            async Task initializeOnboardingChart(CancellationToken cancellationToken)
            {
                try
                {
                    var streamingManifests = await _azureFunctionsApiService.GetStreamingManifests(cancellationToken).ConfigureAwait(false);

                    OnboardingChartManifest = streamingManifests[StreamingConstants.Chart];
                    EnableOrganizationsManifest = streamingManifests[StreamingConstants.EnableOrganizations];
                }
                catch (Exception e)
                {
                    _analyticsService.Report(e);
                }
            }
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
