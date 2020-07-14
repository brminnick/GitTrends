using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class MockLauncher : ILauncher
    {
        readonly static WeakEventManager _openAsyncExecutedEventHandler = new WeakEventManager();

        public static event EventHandler OpenAsyncExecuted
        {
            add => _openAsyncExecutedEventHandler.AddEventHandler(value);
            remove => _openAsyncExecutedEventHandler.RemoveEventHandler(value);
        }

        public Task<bool> TryOpenAsync(string uri) => CanOpenAsync(uri);

        public Task<bool> TryOpenAsync(Uri uri) => CanOpenAsync(uri);

        public Task<bool> CanOpenAsync(string uri) => CanOpenAsync(new Uri(uri));

        public Task<bool> CanOpenAsync(Uri uri) => Task.FromResult(true);

        public Task OpenAsync(string uri) => OpenAsync(new Uri(uri));

        public Task OpenAsync(Uri uri)
        {
            OnOpenAsyncExecuted();
            return Task.CompletedTask;
        }

        public Task OpenAsync(OpenFileRequest request)
        {
            OnOpenAsyncExecuted();
            return Task.CompletedTask;
        }

        void OnOpenAsyncExecuted() => _openAsyncExecutedEventHandler.RaiseEvent(this, EventArgs.Empty, nameof(OpenAsyncExecuted));
    }
}
