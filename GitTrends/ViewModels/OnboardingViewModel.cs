using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Shiny;

namespace GitTrends;

public partial class OnboardingViewModel : GitHubAuthenticationViewModel
{
	static readonly AsyncAwaitBestPractices.WeakEventManager _skipButtonTappedEventManager = new();

	readonly IAnalyticsService _analyticsService;
	readonly NotificationService _notificationService;
	readonly FirstRunService _firstRunService;

	string _notificationStatusSvgImageSource = "";

	public OnboardingViewModel(IDispatcher dispatcher,
								FirstRunService firstRunService,
								IAnalyticsService analyticsService,
								GitHubUserService gitHubUserService,
								DeepLinkingService deepLinkingService,
								NotificationService notificationService,
								GitHubAuthenticationService gitHubAuthenticationService)
			: base(dispatcher, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
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
		private set => SetProperty(ref _notificationStatusSvgImageSource, value);
	}

	protected override async Task HandleDemoButtonTapped(string? buttonText, CancellationToken token)
	{
		try
		{
			await base.HandleDemoButtonTapped(buttonText, token).ConfigureAwait(false);

			if (buttonText == OnboardingConstants.SkipText)
			{
				OnSkipButtonTapped();
			}
			else if (buttonText == OnboardingConstants.TryDemoText)
			{
				AnalyticsService.Track("Onboarding Demo Button Tapped");

				//Allow Activity Indicator to run for a minimum of 1500ms
				var minimumActivityIndicatorTimeSpan = TimeSpan.FromSeconds(1.5);
				await Task.WhenAll(GitHubAuthenticationService.ActivateDemoUser(token), Task.Delay(minimumActivityIndicatorTimeSpan, token)).ConfigureAwait(false);
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
	async Task HandleEnableNotificationsButtonTapped(CancellationToken token)
	{
		const string successSvg = "check.svg";
		const string failSvg = "error.svg";

		var result = await _notificationService.Register(NotificationStatusSvgImageSource == failSvg, token).ConfigureAwait(false);

		NotificationStatusSvgImageSource = isNotificationResultSuccessful(result) ? successSvg : failSvg;

		_analyticsService.Track("Onboarding Notification Button Tapped", "Result", result.ToString());

		static bool isNotificationResultSuccessful(in AccessState result) => result switch
		{
			AccessState.Available or AccessState.Restricted => true,
			_ => false,
		};
	}

	static void OnSkipButtonTapped() => _skipButtonTappedEventManager.RaiseEvent(null, EventArgs.Empty, nameof(SkipButtonTapped));
}