using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Shiny;

namespace GitTrends
{
    class OnboardingViewModel : GitHubAuthenticationViewModel
    {
        readonly AnalyticsService _analyticsService;
        readonly NotificationService _notificationService;

        string _notificationStatusSvgImageSource = "";

        public OnboardingViewModel(DeepLinkingService deepLinkingService,
                                    GitHubAuthenticationService gitHubAuthenticationService,
                                    NotificationService notificationService,
                                    AnalyticsService analyticsService)
                : base(gitHubAuthenticationService, deepLinkingService, analyticsService)
        {
            const string defaultNotificationSvg = "bell.svg";

            _notificationService = notificationService;
            _analyticsService = analyticsService;

            NotificationStatusSvgImageSource = defaultNotificationSvg;

            EnableNotificationsButtonTapped = new AsyncCommand(ExecuteEnableNotificationsButtonTapped);
        }

        public ICommand EnableNotificationsButtonTapped { get; }

        public string NotificationStatusSvgImageSource
        {
            get => _notificationStatusSvgImageSource;
            set => SetProperty(ref _notificationStatusSvgImageSource, SvgService.GetFullPath(value));
        }

        async Task ExecuteEnableNotificationsButtonTapped()
        {
            const string successSvg = "check.svg";
            const string failSvg = "error.svg";

            var result = await _notificationService.Register(NotificationStatusSvgImageSource == SvgService.GetFullPath(failSvg)).ConfigureAwait(false);

            if (isNotificationResultSuccessful(result))
            {
                NotificationStatusSvgImageSource = successSvg;
            }
            else
            {
                NotificationStatusSvgImageSource = failSvg;
            }

            _analyticsService.Track("Onboarding Notification Button Tapped", "Result", result.ToString());

            static bool isNotificationResultSuccessful(in AccessState result)
            {
                switch (result)
                {
                    case AccessState.Available:
                    case AccessState.Restricted:
                        return true;

                    default:
                        return false;
                }
            }
        }
    }
}
