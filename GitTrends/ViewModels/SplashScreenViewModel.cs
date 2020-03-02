using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;

namespace GitTrends
{
    class SplashScreenViewModel : BaseViewModel
    {
        readonly WeakEventManager<InitializationCompleteEventArgs> _initializationCompleteEventManager = new WeakEventManager<InitializationCompleteEventArgs>();

        readonly SyncFusionService _syncFusionService;

        public SplashScreenViewModel(SyncFusionService syncfusionService, AnalyticsService analyticsService) : base(analyticsService)
        {
            _syncFusionService = syncfusionService;

            InitializeAppCommand = new AsyncCommand(ExecuteInitializeAppCommand);
        }

        public event EventHandler<InitializationCompleteEventArgs> InitializationComplete
        {
            add => _initializationCompleteEventManager.AddEventHandler(value);
            remove => _initializationCompleteEventManager.RemoveEventHandler(value);
        }

        public ICommand InitializeAppCommand { get; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task ExecuteInitializeAppCommand()
        {
            bool isInitializationSuccessful = false;

            try
            {
#if DEBUG
                _syncFusionService.Initialize().SafeFireAndForget(ex => Debug.WriteLine(ex));
#else
                await _syncFusionService.Initialize().ConfigureAwait(false);
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
