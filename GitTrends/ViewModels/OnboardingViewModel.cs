using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Shiny;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class OnboardingViewModel : GitHubAuthenticationViewModel
    {
        readonly WeakEventManager _skipButtonTappedEventManager = new WeakEventManager();

        readonly IAnalyticsService _analyticsService;
        readonly NotificationService _notificationService;
        readonly FirstRunService _firstRunService;

        string _notificationStatusSvgImageSource = "";

        public OnboardingViewModel(DeepLinkingService deepLinkingService,
                                    GitHubAuthenticationService gitHubAuthenticationService,
                                    NotificationService notificationService,
                                    IAnalyticsService analyticsService,
                                    IMainThread mainThread,
                                    FirstRunService firstRunService,
                                    GitHubUserService gitHubUserService)
                : base(gitHubAuthenticationService, deepLinkingService, analyticsService, mainThread, gitHubUserService)
        {
            const string defaultNotificationSvg = "bell.svg";

            _notificationService = notificationService;
            _analyticsService = analyticsService;
            _firstRunService = firstRunService;

            NotificationStatusSvgImageSource = defaultNotificationSvg;

            EnableNotificationsButtonTapped = new AsyncCommand(ExecuteEnableNotificationsButtonTapped);
        }

        public event EventHandler SkipButtonTapped
        {
            add => _skipButtonTappedEventManager.AddEventHandler(value);
            remove => _skipButtonTappedEventManager.RemoveEventHandler(value);
        }

        public override bool IsDemoButtonVisible => IsNotAuthenticating;

        public IAsyncCommand EnableNotificationsButtonTapped { get; }

        public string NotificationStatusSvgImageSource
        {
            get => _notificationStatusSvgImageSource;
            set => SetProperty(ref _notificationStatusSvgImageSource, SvgService.GetFullPath(value));
        }

        protected override async Task ExecuteDemoButtonCommand(string? buttonText)
        {
            try
            {
                await base.ExecuteDemoButtonCommand(buttonText).ConfigureAwait(false);

                if (buttonText is OnboardingConstants.SkipText)
                {
                    OnSkipButtonTapped();
                }
                else if (buttonText is OnboardingConstants.TryDemoText)
                {
                    AnalyticsService.Track("Onboarding Demo Button Tapped");

                    //Allow Activity Indicator to run for a minimum of 1500ms
                    await Task.WhenAll(GitHubAuthenticationService.ActivateDemoUser(), Task.Delay(TimeSpan.FromMilliseconds(1500))).ConfigureAwait(false);
                }
                else
                {
                    throw new NotSupportedException($"{nameof(ExecuteDemoButtonCommand)} Does Not Support {buttonText}");
                }
            }
            finally
            {
                IsAuthenticating = false;
            }
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

        void OnSkipButtonTapped() => _skipButtonTappedEventManager.HandleEvent(null, EventArgs.Empty, nameof(SkipButtonTapped));
    }
}
