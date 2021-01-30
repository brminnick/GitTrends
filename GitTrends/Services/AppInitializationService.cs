using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;

namespace GitTrends
{
    public class AppInitializationService
    {
        readonly static WeakEventManager<InitializationCompleteEventArgs> _initializationCompletedEventManager = new();

        readonly NuGetService _nuGetService;
        readonly ThemeService _themeService;
        readonly LanguageService _languageService;
        readonly IAnalyticsService _analyticsService;
        readonly SyncfusionService _syncfusionService;
        readonly MediaElementService _mediaElementService;
        readonly NotificationService _notificationService;
        readonly IDeviceNotificationsService _deviceNotificationsService;
        readonly GitTrendsContributorsService _gitTrendsContributorsService;

        public AppInitializationService(NuGetService nuGetService,
                                        ThemeService themeService,
                                        LanguageService languageService,
                                        IAnalyticsService analyticsService,
                                        SyncfusionService syncFusionService,
                                        MediaElementService mediaElementService,
                                        NotificationService notificationService,
                                        IDeviceNotificationsService deviceNotificationService,
                                        GitTrendsContributorsService gitTrendsContributorsService)
        {
            _nuGetService = nuGetService;
            _themeService = themeService;
            _languageService = languageService;
            _analyticsService = analyticsService;
            _syncfusionService = syncFusionService;
            _mediaElementService = mediaElementService;
            _notificationService = notificationService;
            _deviceNotificationsService = deviceNotificationService;
            _gitTrendsContributorsService = gitTrendsContributorsService;
        }

        public static event EventHandler<InitializationCompleteEventArgs> InitializationCompleted
        {
            add => _initializationCompletedEventManager.AddEventHandler(value);
            remove => _initializationCompletedEventManager.RemoveEventHandler(value);
        }

        public async Task InitializeApp(CancellationToken cancellationToken)
        {
            bool isInitializationSuccessful = false;

            try
            {
                #region First, Initialize Services That Dont Require API Response
                _languageService.Initialize();
                _deviceNotificationsService.Initialize();
                await _themeService.Initialize().ConfigureAwait(false);
                #endregion

                #region Then, Initialize Services Requiring API Response
                var initializeNuGetServiceTask = _nuGetService.Initialize(cancellationToken);
                var initializeSyncFusionServiceTask = _syncfusionService.Initialize(cancellationToken);
                var initializeNotificationServiceTask = _notificationService.Initialize(cancellationToken);
                var intializeOnboardingChartValueTask = _mediaElementService.InitializeOnboardingChart(cancellationToken);
                var initializeGitTrendsContributorsTask = _gitTrendsContributorsService.Initialize(cancellationToken);
#if DEBUG
                initializeNuGetServiceTask.SafeFireAndForget(ex => _analyticsService.Report(ex));
                initializeSyncFusionServiceTask.SafeFireAndForget(ex => _analyticsService.Report(ex));
                initializeNotificationServiceTask.SafeFireAndForget(ex => _analyticsService.Report(ex));
#else
                await initializeNuGetServiceTask.ConfigureAwait(false);
                await initializeSyncfusionServiceTask.ConfigureAwait(false);
                await initializeNotificationServiceTask.ConfigureAwait(false);
#endif

                await intializeOnboardingChartValueTask.ConfigureAwait(false);
                await initializeGitTrendsContributorsTask.ConfigureAwait(false);
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
        }

        void OnInitializationCompleted(bool isInitializationSuccessful) =>
            _initializationCompletedEventManager.RaiseEvent(this, new InitializationCompleteEventArgs(isInitializationSuccessful), nameof(InitializationCompleted));
    }
}
