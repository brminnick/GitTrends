using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace GitTrends
{
    class MediaElementService
    {
        readonly AzureFunctionsApiService _azureFunctionsApiService;
        readonly AnalyticsService _analyticsService;

        public MediaElementService(AzureFunctionsApiService azureFunctionsApiService, AnalyticsService analyticsService) =>
            (_azureFunctionsApiService, _analyticsService) = (azureFunctionsApiService, analyticsService);

        public static StreamingUrl? OnboardingChart
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<StreamingUrl?>(Preferences.Get(nameof(OnboardingChart), null));
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
                    Preferences.Set(nameof(OnboardingChart), JsonConvert.SerializeObject(chartVideoStreamingUrl));
                }
                catch (Exception e)
                {
                    _analyticsService.Report(e);
                }
            }
        }
    }
}
