using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Common;
using GitTrends.Mobile.Common.Constants;
using Shiny;

namespace GitTrends;

public partial class OnboardingViewModel(
	IDispatcher dispatcher,
	IAnalyticsService analyticsService,
	GitHubUserService gitHubUserService,
	DeepLinkingService deepLinkingService,
	NotificationService notificationService,
	GitHubAuthenticationService gitHubAuthenticationService)
	: GitHubAuthenticationViewModel(dispatcher, analyticsService, gitHubUserService, deepLinkingService, gitHubAuthenticationService)
{
	public const string BellSvgSourceName = "bell.svg";
	public const string SuccessSvgSourceName = "check.svg";
	public const string FailSvgSourceName = "error.svg";

	static readonly AsyncAwaitBestPractices.WeakEventManager _skipButtonTappedEventManager = new();

	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly NotificationService _notificationService = notificationService;

    public static event EventHandler SkipButtonTapped
	{
		add => _skipButtonTappedEventManager.AddEventHandler(value);
		remove => _skipButtonTappedEventManager.RemoveEventHandler(value);
	}

	public override bool IsDemoButtonVisible => IsNotAuthenticating;
	
	[ObservableProperty]
	public partial string NotificationStatusSvgImageSource { get; set; } = BellSvgSourceName;

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
		var result = await _notificationService.Register(NotificationStatusSvgImageSource == FailSvgSourceName, token).ConfigureAwait(false);

		NotificationStatusSvgImageSource = isNotificationResultSuccessful(result) ? SuccessSvgSourceName : FailSvgSourceName;

		_analyticsService.Track("Onboarding Notification Button Tapped", "Result", result.ToString());

		static bool isNotificationResultSuccessful(in AccessState result) => result switch
		{
			AccessState.Available or AccessState.Restricted => true,
			_ => false,
		};
	}

	static void OnSkipButtonTapped() => _skipButtonTappedEventManager.RaiseEvent(null, EventArgs.Empty, nameof(SkipButtonTapped));
}