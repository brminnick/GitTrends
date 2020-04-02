using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class OnboardingPage : CarouselPage
    {
        readonly AnalyticsService _analyticsService;

        public OnboardingPage(OnboardingViewModel onboardingViewModel, AnalyticsService analyticsService)
        {
            On<iOS>().SetUseSafeArea(true);

            BindingContext = onboardingViewModel;
            _analyticsService = analyticsService;

            Children.Add(new WelcomePage());
        }
    }
}
