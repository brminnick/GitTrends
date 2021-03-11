using System;
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

        public StreamingManifest? OnboardingChart
        {
            get => GetOnboardingChart();
            private set => _preferences.Set(nameof(OnboardingChart), JsonConvert.SerializeObject(value));
        }

        public async ValueTask InitializeOnboardingChart(CancellationToken cancellationToken)
        {
            if (OnboardingChart is null)
                await initializeOnboardingChart(cancellationToken).ConfigureAwait(false);

            initializeOnboardingChart(cancellationToken).SafeFireAndForget();

            async Task initializeOnboardingChart(CancellationToken cancellationToken)
            {
                try
                {
                    var chartVideoStreamingUrl = await _azureFunctionsApiService.GetChartStreamingUrl(cancellationToken).ConfigureAwait(false);
                    OnboardingChart = chartVideoStreamingUrl;
                }
                catch (Exception e)
                {
                    _analyticsService.Report(e);
                }
            }
        }

        StreamingManifest? GetOnboardingChart()
        {
            try
            {
                return JsonConvert.DeserializeObject<StreamingManifest?>(_preferences.Get(nameof(OnboardingChart), null));
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
    }
}
