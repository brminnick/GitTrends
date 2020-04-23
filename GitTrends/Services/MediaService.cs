using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Xamarin.Essentials;

namespace GitTrends
{
    class MediaElementService
    {
        readonly AzureFunctionsApiService _azureFunctionsApiService;
        readonly AnalyticsService _analyticsService;

        public MediaElementService(AzureFunctionsApiService azureFunctionsApiService, AnalyticsService analyticsService) =>
            (_azureFunctionsApiService, _analyticsService) = (azureFunctionsApiService, analyticsService);

        public static string OnboardingChartUrl => Preferences.Get(nameof(OnboardingChartUrl), string.Empty);

        public async ValueTask InitializeOnboardingChart(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(OnboardingChartUrl))
                initializeOnboardingChart(cancellationToken).SafeFireAndForget();

            await initializeOnboardingChart(cancellationToken).ConfigureAwait(false);

            async Task initializeOnboardingChart(CancellationToken cancellationToken)
            {
                try
                {
                    var chartVideo = await _azureFunctionsApiService.GetChartVideoUrl(cancellationToken).ConfigureAwait(false);
                    Preferences.Set(nameof(OnboardingChartUrl), chartVideo.Url);
                }
                catch (Exception e)
                {
                    _analyticsService.Report(e);
                }
            }
        }
    }
}
