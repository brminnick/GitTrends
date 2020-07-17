using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class SplashScreenViewModel : BaseViewModel
    {
        readonly static WeakEventManager<InitializationCompleteEventArgs> _initializationCompletedEventManager = new WeakEventManager<InitializationCompleteEventArgs>();

        public SplashScreenViewModel(SyncfusionService syncfusionService,
                                        MediaElementService mediaElementService,
                                        IAnalyticsService analyticsService,
                                        NotificationService notificationService,
                                        IMainThread mainThread) : base(analyticsService, mainThread)
        {
            InitializeAppCommand = new AsyncCommand(() => ExecuteInitializeAppCommand(syncfusionService, mediaElementService, notificationService));
        }

        public static event EventHandler<InitializationCompleteEventArgs> InitializationCompleted
        {
            add => _initializationCompletedEventManager.AddEventHandler(value);
            remove => _initializationCompletedEventManager.RemoveEventHandler(value);
        }

        public IAsyncCommand InitializeAppCommand { get; }

        async Task ExecuteInitializeAppCommand(SyncfusionService syncFusionService, MediaElementService mediaElementService, NotificationService notificationService)
        {
            bool isInitializationSuccessful = false;

            try
            {
                var initializeSyncfusionTask = syncFusionService.Initialize(CancellationToken.None);
                var intializeOnboardingChartValueTask = mediaElementService.InitializeOnboardingChart(CancellationToken.None);
                var initializeNotificationServiceTask = notificationService.Initialize(CancellationToken.None);
#if DEBUG
                initializeSyncfusionTask.SafeFireAndForget(ex => AnalyticsService.Report(ex));
                initializeNotificationServiceTask.SafeFireAndForget(ex => AnalyticsService.Report(ex));
#else
                await Task.WhenAll(initializeNotificationServiceTask, initializeSyncfusionTask).ConfigureAwait(false);
#endif
                await intializeOnboardingChartValueTask.ConfigureAwait(false);

                isInitializationSuccessful = true;
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e);
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
