using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class OnboardingCarouselPage : CarouselPage
    {
        readonly AnalyticsService _analyticsService;
        readonly OnboardingViewModel _onboardingViewModel;

        public OnboardingCarouselPage(GitTrendsOnboardingPage welcomeOnboardingPage,
                                        ChartOnboardingPage chartOnboardingPage,
                                        NotificationsOnboardingPage notificationsOnboardingPage,
                                        ConnectToGitHubOnboardingPage connectToGitHubOnboardingPage,
                                        OnboardingViewModel onboardingViewModel,
                                        GitHubAuthenticationService gitHubAuthenticationService,
                                        AnalyticsService analyticsService)
        {
            ChildAdded += HandleChildAdded;

            BindingContext = _onboardingViewModel = onboardingViewModel;
            _analyticsService = analyticsService;

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

            _onboardingViewModel.IsAuthenticating = false;

            _analyticsService.Track($"{GetType().Name} Disappeared");
        }

        void HandleSkipButtonTapped(object sender, EventArgs e)
        {
            _analyticsService.Track("Skip Button Tapped");

            MainThread.BeginInvokeOnMainThread(() => CurrentPage = Children.Last());
        }

        void HandleDemoUserActivated(object sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());

        void HandleChildAdded(object sender, ElementEventArgs e) => OnPropertyChanged(nameof(PageCount));
    }
}
