using AsyncAwaitBestPractices;
using GitTrends.Common;

namespace GitTrends;

public class AppInitializationService(ThemeService themeService,
										LanguageService languageService,
										LibrariesService librariesService,
										IAnalyticsService analyticsService,
										SyncfusionService syncFusionService,
										NotificationService notificationService,
										BackgroundFetchService backgroundFetchService,
										GitTrendsStatisticsService gitTrendsStatisticsService)
{
	static readonly WeakEventManager<InitializationCompleteEventArgs> _initializationCompletedEventManager = new();

	readonly ThemeService _themeService = themeService;
	readonly LanguageService _languageService = languageService;
	readonly LibrariesService _librariesService = librariesService;
	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly SyncfusionService _syncfusionService = syncFusionService;
	readonly NotificationService _notificationService = notificationService;
	readonly BackgroundFetchService _backgroundFetchService = backgroundFetchService;
	readonly GitTrendsStatisticsService _gitTrendsStatisticsService = gitTrendsStatisticsService;

	public static event EventHandler<InitializationCompleteEventArgs> InitializationCompleted
	{
		add => _initializationCompletedEventManager.AddEventHandler(value);
		remove => _initializationCompletedEventManager.RemoveEventHandler(value);
	}

	public bool IsInitializationComplete { get; private set; }

	public async Task<bool> InitializeApp(CancellationToken cancellationToken)
	{
		bool isInitializationSuccessful = false;

		try
		{
			#region First, Initialize Services That Dont Require API Response
			_languageService.Initialize();
			_notificationService.Initialize();
			_backgroundFetchService.Initialize();
			await _themeService.Initialize().ConfigureAwait(false);
			#endregion

			#region Then, Initialize Services Requiring API Response
			var initializeSyncFusionServiceTask = _syncfusionService.Initialize(cancellationToken);
			var initializeLibrariesServiceValueTask = _librariesService.Initialize(cancellationToken);
			var initializeGitTrendsStatisticsValueTask = _gitTrendsStatisticsService.Initialize(cancellationToken);
			await initializeGitTrendsStatisticsValueTask.ConfigureAwait(false);
#if DEBUG
			initializeSyncFusionServiceTask.SafeFireAndForget(ex => _analyticsService.Report(ex));
			initializeLibrariesServiceValueTask.SafeFireAndForget(ex => _analyticsService.Report(ex));
#else
			await initializeSyncFusionServiceTask.ConfigureAwait(false);
			await initializeLibrariesServiceValueTask.ConfigureAwait(false);
#endif

			#endregion

			isInitializationSuccessful = true;
		}
		catch (Exception e)
		{
			_analyticsService.Report(e);
		}
		finally
		{
			OnInitializationCompleted(isInitializationSuccessful);
		}

		return isInitializationSuccessful;
	}

	void OnInitializationCompleted(bool isInitializationSuccessful)
	{
		IsInitializationComplete = isInitializationSuccessful;
		_initializationCompletedEventManager.RaiseEvent(this, new InitializationCompleteEventArgs(isInitializationSuccessful), nameof(InitializationCompleted));
	}
}