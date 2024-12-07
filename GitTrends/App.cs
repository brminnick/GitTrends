using GitTrends.Common;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

class App : Microsoft.Maui.Controls.Application
{
	static readonly AsyncAwaitBestPractices.WeakEventManager _resumedEventManager = new();

	readonly AppShell _appShell;
	readonly IAnalyticsService _analyticsService;
	readonly NotificationService _notificationService;
	readonly AppInitializationService _appInitializationService;

	public App(AppShell appShell,
		IAnalyticsService analyticsService,
		NotificationService notificationService,
		AppInitializationService appInitializationService)
	{
		_appShell = appShell;
		_analyticsService = analyticsService;
		_notificationService = notificationService;
		_appInitializationService = appInitializationService;

		analyticsService.Track("App Initialized");

		On<iOS>().SetHandleControlUpdatesOnMainThread(true);
	}

	public static event EventHandler Resumed
	{
		add => _resumedEventManager.AddEventHandler(value);
		remove => _resumedEventManager.RemoveEventHandler(value);
	}

	protected override Window CreateWindow(IActivationState? activationState) => new(_appShell);

	protected override async void OnStart()
	{
		base.OnStart();

		_analyticsService.Track("App Started");

		await ClearBadgeNotifications(CancellationToken.None);

		var appInitializationCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
		await _appInitializationService.InitializeApp(appInitializationCancellationTokenSource.Token);
	}

	protected override async void OnResume()
	{
		base.OnResume();

		OnResumed();

		_analyticsService.Track("App Resumed");

		await ClearBadgeNotifications(CancellationToken.None);
	}

	protected override void OnSleep()
	{
		base.OnSleep();

		_analyticsService.Track("App Backgrounded");
	}

	ValueTask ClearBadgeNotifications(CancellationToken token) => _notificationService.SetAppBadgeCount(0, token);

	void OnResumed() => _resumedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(Resumed));
}