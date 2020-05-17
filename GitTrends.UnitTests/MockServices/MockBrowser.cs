using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class MockBrowser : IBrowser
    {
        public Task OpenAsync(string uri) => OpenAsync(new Uri(uri));

        public Task OpenAsync(string uri, BrowserLaunchMode launchMode) => OpenAsync(new Uri(uri), new BrowserLaunchOptions { LaunchMode = launchMode });

        public Task OpenAsync(string uri, BrowserLaunchOptions options) => OpenAsync(new Uri(uri), options);

        public Task OpenAsync(Uri uri) => OpenAsync(uri, new BrowserLaunchMode());

        public Task OpenAsync(Uri uri, BrowserLaunchMode launchMode) => OpenAsync(uri, new BrowserLaunchOptions { LaunchMode = launchMode });

        public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => Task.FromResult(true);
    }
}
