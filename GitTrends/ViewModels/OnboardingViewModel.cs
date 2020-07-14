using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Shiny;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class OnboardingViewModel : GitHubAuthenticationViewModel
    {
        readonly static WeakEventManager _skipButtonTappedEventManager = new WeakEventManager();

        readonly IAnalyticsService _analyticsService;
        readonly NotificationService _notificationService;
        readonly FirstRunService _firstRunService;

        string _notificationStatusSvgImageSource = "";

        public OnboardingViewModel(IMainThread mainThread,
                                    FirstRunService firstRunService,
                                    IAnalyticsService analyticsService,
                                    GitHubUserService gitHubUserService,
                                    DeepLinkingService deepLinkingService,
                                    NotificationService notificationService,
                                    GitHubAuthenticationService gitHubAuthenticationService)
                : base(mainThread, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
        {
            const string defaultNotificationSvg = "bell.svg";

            _notificationService = notificationService;
            _analyticsService = analyticsService;
            _firstRunService = firstRunService;

            NotificationStatusSvgImageSource = defaultNotificationSvg;

            EnableNotificationsButtonTapped = new AsyncCommand(ExecuteEnableNotificationsButtonTapped);
        }

        public static event EventHandler SkipButtonTapped
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

                if (buttonText == OnboardingConstants.SkipText)
                {
                    OnSkipButtonTapped();
                }
                else if (buttonText == OnboardingConstants.TryDemoText)
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

        void OnSkipButtonTapped() => _skipButtonTappedEventManager.RaiseEvent(null, EventArgs.Empty, nameof(SkipButtonTapped));
    }
}
