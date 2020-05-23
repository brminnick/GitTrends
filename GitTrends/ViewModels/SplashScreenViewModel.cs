using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    class SplashScreenViewModel : BaseViewModel
    {
        readonly WeakEventManager<InitializationCompleteEventArgs> _initializationCompleteEventManager = new WeakEventManager<InitializationCompleteEventArgs>();

        public SplashScreenViewModel(SyncFusionService syncfusionService,
                                        MediaElementService mediaElementService,
                                        IAnalyticsService analyticsService,
                                        NotificationService notificationService,
                                        IMainThread mainThread) : base(analyticsService, mainThread)
        {
            InitializeAppCommand = new AsyncCommand(() => ExecuteInitializeAppCommand(syncfusionService, mediaElementService, notificationService));
        }

        public event EventHandler<InitializationCompleteEventArgs> InitializationComplete
        {
            add => _initializationCompleteEventManager.AddEventHandler(value);
            remove => _initializationCompleteEventManager.RemoveEventHandler(value);
        }

        public ICommand InitializeAppCommand { get; }

        async Task ExecuteInitializeAppCommand(SyncFusionService syncFusionService, MediaElementService mediaElementService, NotificationService notificationService)
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
                OnInitializationComplete(isInitializationSuccessful);
            }
        }

        void OnInitializationComplete(bool isInitializationSuccessful) =>
            _initializationCompleteEventManager.HandleEvent(this, new InitializationCompleteEventArgs(isInitializationSuccessful), nameof(InitializationComplete));
    }
}
