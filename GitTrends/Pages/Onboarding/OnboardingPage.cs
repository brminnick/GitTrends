using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    class OnboardingPage : CarouselPage
    {
        readonly AnalyticsService _analyticsService;

        public OnboardingPage(WelcomePage welcomePage,
                                OnboardingViewModel onboardingViewModel,
                                AnalyticsService analyticsService)
        {
            On<iOS>().SetUseSafeArea(true);

            BindingContext = onboardingViewModel;
            _analyticsService = analyticsService;

            Children.Add(welcomePage);
        }
    }
}
