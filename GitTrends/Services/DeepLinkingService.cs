using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class DeepLinkingService
    {
        readonly AnalyticsService _analyticsService;

        public DeepLinkingService(AnalyticsService analyticsService) => _analyticsService = analyticsService;

        public Task ShowSettingsUI() => Device.InvokeOnMainThreadAsync(AppInfo.ShowSettingsUI);

        public Task OpenApp(string deepLinkingUrl, string browserUrl)
        {
            return Device.InvokeOnMainThreadAsync(async () =>
            {
                var supportsUri = await Launcher.CanOpenAsync(deepLinkingUrl);

                if (supportsUri)
                {
                    await Launcher.OpenAsync(deepLinkingUrl);
                }
                else
                {
                    var currentTheme = (BaseTheme)Application.Current.Resources;

                    var browserLaunchOptions = new BrowserLaunchOptions
                    {
                        PreferredControlColor = currentTheme.NavigationBarTextColor,
                        PreferredToolbarColor = currentTheme.NavigationBarBackgroundColor,
                    };

                    await Browser.OpenAsync(browserUrl, browserLaunchOptions);
                }
            });
        }

        public Task SendEmail(string subject, string body, List<string> recipients)
        {
            return Device.InvokeOnMainThreadAsync(() =>
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
    }
}
