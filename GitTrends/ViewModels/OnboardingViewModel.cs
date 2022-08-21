using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Shiny;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public partial class OnboardingViewModel : GitHubAuthenticationViewModel
	{
		readonly static WeakEventManager _skipButtonTappedEventManager = new();

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
		}

		public static event EventHandler SkipButtonTapped
		{
			add => _skipButtonTappedEventManager.AddEventHandler(value);
			remove => _skipButtonTappedEventManager.RemoveEventHandler(value);
		}

		public override bool IsDemoButtonVisible => IsNotAuthenticating;

		public string NotificationStatusSvgImageSource
		{
			get => _notificationStatusSvgImageSource;
			private set => SetProperty(ref _notificationStatusSvgImageSource, SvgService.GetFullPath(value));
		}

		protected override async Task HandleDemoButtonTapped(string? buttonText)
		{
			try
			{
				await base.HandleDemoButtonTapped(buttonText).ConfigureAwait(false);

				if (buttonText == OnboardingConstants.SkipText)
				{
					OnSkipButtonTapped();
				}
				else if (buttonText == OnboardingConstants.TryDemoText)
				{
					AnalyticsService.Track("Onboarding Demo Button Tapped");

					//Allow Activity Indicator to run for a minimum of 1500ms
					var minimumActivityIndicatorTimeSpan = TimeSpan.FromSeconds(1.5);
					await Task.WhenAll(GitHubAuthenticationService.ActivateDemoUser(), Task.Delay(minimumActivityIndicatorTimeSpan)).ConfigureAwait(false);
				}
				else
				{
					throw new NotSupportedException($"{nameof(HandleDemoButtonTapped)} Does Not Support {buttonText}");
				}
			}
			finally
			{
				IsAuthenticating = false;
			}
		}

		[RelayCommand]
		async Task HandleEnableNotificationsButtonTapped()
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

			static bool isNotificationResultSuccessful(in AccessState result) => result switch
			{
				AccessState.Available or AccessState.Restricted => true,
				_ => false,
			};
		}

		static void OnSkipButtonTapped() => _skipButtonTappedEventManager.RaiseEvent(null, EventArgs.Empty, nameof(SkipButtonTapped));
	}
}