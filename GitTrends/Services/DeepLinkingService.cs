using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class DeepLinkingService
    {
        readonly AnalyticsService _analyticsService;

        public DeepLinkingService(AnalyticsService analyticsService) => _analyticsService = analyticsService;

        public Task ShowSettingsUI() => MainThread.InvokeOnMainThreadAsync(AppInfo.ShowSettingsUI);

        public Task DisplayAlert(string title, string message, string cancel) => MainThread.InvokeOnMainThreadAsync(() => Application.Current.MainPage.DisplayAlert(title, message, cancel));

        public Task<bool> DisplayAlert(string title, string message, string accept, string decline) => MainThread.InvokeOnMainThreadAsync(() => Application.Current.MainPage.DisplayAlert(title, message, accept, decline));

        public Task OpenBrowser(Uri uri) => OpenBrowser(uri.ToString());

        public Task OpenBrowser(string url)
        {
            return MainThread.InvokeOnMainThreadAsync(() =>
            {
                var currentTheme = (BaseTheme)Application.Current.Resources;

                var browserLaunchOptions = new BrowserLaunchOptions
                {
                    PreferredControlColor = currentTheme.NavigationBarTextColor,
                    PreferredToolbarColor = currentTheme.NavigationBarBackgroundColor,
                };

                return Browser.OpenAsync(url, browserLaunchOptions);
            });
        }

        public Task NavigateToTrendsPage(Repository repository)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var trendsPage = scope.Resolve<TrendsPage>(new TypedParameter(typeof(Repository), repository));

            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var baseNavigationPage = await GetBaseNavigationPage();

                await baseNavigationPage.PopToRootAsync();
                await baseNavigationPage.Navigation.PushAsync(trendsPage);
            });
        }

        public Task OpenApp(string deepLinkingUrl, string browserUrl)
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var supportsUri = await Launcher.CanOpenAsync(deepLinkingUrl);

                if (supportsUri)
                    await Launcher.OpenAsync(deepLinkingUrl);
                else
                    await OpenBrowser(browserUrl);
            });
        }

        public Task SendEmail(string subject, string body, List<string> recipients)
        {
            return MainThread.InvokeOnMainThreadAsync(() =>
            {
                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = recipients
                };

                return Email.ComposeAsync(message);
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
