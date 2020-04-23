using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;

namespace GitTrends
{
    class SplashScreenViewModel : BaseViewModel
    {
        readonly WeakEventManager<InitializationCompleteEventArgs> _initializationCompleteEventManager = new WeakEventManager<InitializationCompleteEventArgs>();

        public SplashScreenViewModel(SyncFusionService syncfusionService,
                                        MediaElementService mediaElementService,
                                        AnalyticsService analyticsService) : base(analyticsService)
        {
            InitializeAppCommand = new AsyncCommand(() => ExecuteInitializeAppCommand(syncfusionService, mediaElementService));
        }

        public event EventHandler<InitializationCompleteEventArgs> InitializationComplete
        {
            add => _initializationCompleteEventManager.AddEventHandler(value);
            remove => _initializationCompleteEventManager.RemoveEventHandler(value);
        }

        public ICommand InitializeAppCommand { get; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task ExecuteInitializeAppCommand(SyncFusionService syncFusionService, MediaElementService mediaElementService)
        {
            bool isInitializationSuccessful = false;


            try
            {
                var initializeSyncfusionTask = syncFusionService.Initialize(CancellationToken.None);
                var intializeOnboardingChartValueTask = mediaElementService.InitializeOnboardingChart(CancellationToken.None);
#if DEBUG
                initializeSyncfusionTask.SafeFireAndForget(ex => AnalyticsService.Report(ex));
                await intializeOnboardingChartValueTask.ConfigureAwait(false);
#else
                await Task.WhenAll(initializeSyncfusionTask, intializeOnboardingChartValueTask).ConfigureAwait(false);
#endif

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
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        void OnInitializationComplete(bool isInitializationSuccessful) =>
            _initializationCompleteEventManager.HandleEvent(this, new InitializationCompleteEventArgs(isInitializationSuccessful), nameof(InitializationComplete));
    }
}
