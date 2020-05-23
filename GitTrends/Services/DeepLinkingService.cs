using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class DeepLinkingService
    {
        readonly IAppInfo _appInfo;
        readonly IEmail _email;
        readonly IBrowser _browser;
        readonly IMainThread _mainThread;
        readonly ILauncher _launcher;

        public DeepLinkingService(IMainThread mainThread, IBrowser browser, IEmail email, IAppInfo appInfo, ILauncher launcher)
        {
            _appInfo = appInfo;
            _email = email;
            _browser = browser;
            _mainThread = mainThread;
            _launcher = launcher;
        }

        public Task ShowSettingsUI() => _mainThread.InvokeOnMainThreadAsync(_appInfo.ShowSettingsUI);

        public Task DisplayAlert(string title, string message, string cancel)
        {
            if (Application.Current is null)
                return Task.CompletedTask;

            return _mainThread.InvokeOnMainThreadAsync(() => Application.Current.MainPage.DisplayAlert(title, message, cancel));
        }

        public Task<bool> DisplayAlert(string title, string message, string accept, string decline)
        {
            if (Application.Current is null)
                return Task.FromResult(true);

            return _mainThread.InvokeOnMainThreadAsync(() => Application.Current.MainPage.DisplayAlert(title, message, accept, decline));
        }

        public Task OpenBrowser(Uri uri) => OpenBrowser(uri.ToString());

        public Task OpenBrowser(string url, Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions = null)
        {
            return _mainThread.InvokeOnMainThreadAsync(() =>
            {
                var currentTheme = (BaseTheme)Application.Current.Resources;

                browserLaunchOptions ??= new Xamarin.Essentials.BrowserLaunchOptions
                {
                    PreferredControlColor = currentTheme.NavigationBarTextColor,
                    PreferredToolbarColor = currentTheme.NavigationBarBackgroundColor,
                    Flags = Xamarin.Essentials.BrowserLaunchFlags.PresentAsFormSheet
                };

                return _browser.OpenAsync(url, browserLaunchOptions);
            });
        }

        public Task NavigateToTrendsPage(Repository repository)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var trendsPage = scope.Resolve<TrendsPage>(new TypedParameter(typeof(Repository), repository));

            return _mainThread.InvokeOnMainThreadAsync(async () =>
            {
                var baseNavigationPage = await GetBaseNavigationPage();

                await baseNavigationPage.PopToRootAsync();
                await baseNavigationPage.Navigation.PushAsync(trendsPage);
            });
        }

        public Task OpenApp(string deepLinkingUrl, string browserUrl) => OpenApp(deepLinkingUrl, deepLinkingUrl, browserUrl);

        public Task OpenApp(string appScheme, string deepLinkingUrl, string browserUrl)
        {
            return _mainThread.InvokeOnMainThreadAsync(async () =>
            {
                var supportsUri = await _launcher.CanOpenAsync(appScheme);

                if (supportsUri)
                    await _launcher.OpenAsync(deepLinkingUrl);
                else
                    await OpenBrowser(browserUrl);
            });
        }

        public Task SendEmail(string subject, string body, IEnumerable<string> recipients)
        {
            return _mainThread.InvokeOnMainThreadAsync(async () =>
            {
                var message = new Xamarin.Essentials.EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = recipients.ToList()
                };

                try
                {
                    await _email.ComposeAsync(message).ConfigureAwait(false);
                }
                catch (Xamarin.Essentials.FeatureNotSupportedException)
                {
                    await DisplayAlert("No Email Client Found", "We'd love to hear your fedback!\nsupport@GitTrends.com", "OK").ConfigureAwait(false);
                }
            });
        }

        async ValueTask<BaseNavigationPage> GetBaseNavigationPage()
        {
            if (Application.Current.MainPage is BaseNavigationPage baseNavigationPage)
                return baseNavigationPage;

            var tcs = new TaskCompletionSource<BaseNavigationPage>();

            Application.Current.PageAppearing += HandlePageAppearing;

            return await tcs.Task.ConfigureAwait(false);

            void HandlePageAppearing(object sender, Page page)
            {
                if (page is BaseNavigationPage baseNavigationPage)
                {
                    Application.Current.PageAppearing -= HandlePageAppearing;
                    tcs.SetResult(baseNavigationPage);
                }
                else if (page.Parent is BaseNavigationPage baseNavigation)
                {
                    Application.Current.PageAppearing -= HandlePageAppearing;
                    tcs.SetResult(baseNavigation);
                }
            }
        }
    }
}
