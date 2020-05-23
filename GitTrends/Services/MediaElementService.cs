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
        readonly AzureFunctionsApiService _azureFunctionsApiService;
        readonly IAnalyticsService _analyticsService;
        readonly IPreferences _preferences;

        public MediaElementService(AzureFunctionsApiService azureFunctionsApiService,
                                    IAnalyticsService analyticsService,
                                    IPreferences preferences)
        {
            _azureFunctionsApiService = azureFunctionsApiService;
            _analyticsService = analyticsService;
            _preferences = preferences;
        }

        public StreamingManifest? OnboardingChart
        {
            get
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
                    _preferences.Set(nameof(OnboardingChart), JsonConvert.SerializeObject(chartVideoStreamingUrl));
                }
                catch (Exception e)
                {
                    _analyticsService.Report(e);
                }
            }
        }
    }
}
