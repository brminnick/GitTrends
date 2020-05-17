using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class MockLauncher : ILauncher
    {
        public Task<bool> CanOpenAsync(string uri) => Task.FromResult(true);

        public Task<bool> CanOpenAsync(Uri uri) => Task.FromResult(true);

        public Task OpenAsync(string uri) => Task.CompletedTask;

        public Task OpenAsync(Uri uri) => Task.CompletedTask;

        public Task OpenAsync(OpenFileRequest request) => Task.CompletedTask;

        public Task<bool> TryOpenAsync(string uri) => Task.FromResult(true);

        public Task<bool> TryOpenAsync(Uri uri) => Task.FromResult(true);
    }
}
