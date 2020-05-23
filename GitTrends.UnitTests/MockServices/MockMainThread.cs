using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class MockMainThread : IMainThread
    {
        public bool IsMainThread => true;

        public void BeginInvokeOnMainThread(Action action) => action();

        public Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync() => Task.FromResult(SynchronizationContext.Current ?? new SynchronizationContext());

        public Task InvokeOnMainThreadAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
        {
            var result = func();
            return Task.FromResult(result);
        }

        public Task InvokeOnMainThreadAsync(Func<Task> funcTask) => funcTask();

        public Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask) => funcTask();
    }
}
