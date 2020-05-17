using System;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class OnboardingCarouselPage : CarouselPage
    {
        readonly IAnalyticsService _analyticsService;
        readonly IMainThread _mainThread;

        public OnboardingCarouselPage(GitTrendsOnboardingPage welcomeOnboardingPage,
                                        ChartOnboardingPage chartOnboardingPage,
                                        NotificationsOnboardingPage notificationsOnboardingPage,
                                        ConnectToGitHubOnboardingPage connectToGitHubOnboardingPage,
                                        OnboardingViewModel onboardingViewModel,
                                        GitHubAuthenticationService gitHubAuthenticationService,
                                        IAnalyticsService analyticsService,
                                        IMainThread mainThread)
        {
            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            ChildAdded += HandleChildAdded;
            ChildRemoved += HandleChildRemoved;

            BindingContext = onboardingViewModel;
            _analyticsService = analyticsService;
            _mainThread = mainThread;

            onboardingViewModel.SkipButtonTapped += HandleSkipButtonTapped;
            gitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

            Children.Add(welcomeOnboardingPage);
            Children.Add(chartOnboardingPage);
            Children.Add(notificationsOnboardingPage);
            Children.Add(connectToGitHubOnboardingPage);
        }

        public int PageCount => Children.Count;

        //Disable the Hardware Back Button on Android
        protected override bool OnBackButtonPressed() => false;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _analyticsService.Track($"{GetType().Name} Appeared");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _analyticsService.Track($"{GetType().Name} Disappeared");
        }

        void HandleSkipButtonTapped(object sender, EventArgs e)
        {
            _analyticsService.Track("Skip Button Tapped");

            _mainThread.BeginInvokeOnMainThread(() => CurrentPage = Children.Last());
        }

        void HandleDemoUserActivated(object sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());

        void HandleChildAdded(object sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
        void HandleChildRemoved(object sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
    }
}
