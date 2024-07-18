using GitTrends.Shared;

namespace GitTrends
{
	public class DeepLinkingService(
		IEmail email,
		IBrowser browser,
		IAppInfo appInfo,
		ILauncher launcher,
		IDispatcher mainThread)
	{
		readonly IEmail _email = email;
		readonly IAppInfo _appInfo = appInfo;
		readonly IBrowser _browser = browser;
		readonly ILauncher _launcher = launcher;
		readonly IDispatcher _dispatcher = mainThread;

		public Task ShowSettingsUI(CancellationToken token) => _dispatcher.DispatchAsync(_appInfo.ShowSettingsUI).WaitAsync(token);

		public Task DisplayAlert(string title, string message, string cancel, CancellationToken token)
		{
			if (Application.Current?.MainPage is not null)
				return _dispatcher.DispatchAsync(() => Application.Current.MainPage.DisplayAlert(title, message, cancel)).WaitAsync(token);
			else
				return Task.CompletedTask;
		}

		public Task<bool?> DisplayAlert(string title, string message, string accept, string decline, CancellationToken token)
		{
			if (Application.Current?.MainPage is not null)
			{
				return _dispatcher.DispatchAsync(async () =>
				{
					var result = await Application.Current.MainPage.DisplayAlert(title, message, accept, decline);
					return (bool?)result;
				}).WaitAsync(token);
			}
			else
			{
				return Task.FromResult((bool?)null);
			}
		}

		public Task OpenBrowser(Uri uri, CancellationToken token) => OpenBrowser(uri.ToString(), token);

		public Task OpenBrowser(string url, CancellationToken token, BrowserLaunchOptions? browserLaunchOptions = null) => _dispatcher.DispatchAsync(() =>
		{
			var currentTheme = (BaseTheme?)Application.Current?.Resources;

			if (currentTheme is not null)
			{
				browserLaunchOptions ??= new BrowserLaunchOptions
				{
					PreferredControlColor = currentTheme.NavigationBarTextColor,
					PreferredToolbarColor = currentTheme.NavigationBarBackgroundColor,
					Flags = BrowserLaunchFlags.PresentAsFormSheet
				};
			}

			return browserLaunchOptions is null ? _browser.OpenAsync(url) : _browser.OpenAsync(url, browserLaunchOptions);

		}).WaitAsync(token);

		public Task NavigateToTrendsPage(Repository repository, CancellationToken token)
		{
			if (IPlatformApplication.Current is not null)
			{
				throw new NotImplementedException("Use Shell Navigation");
			}

			return Task.CompletedTask;
		}

		public Task OpenApp(string deepLinkingUrl, string browserUrl, CancellationToken token) => OpenApp(deepLinkingUrl, deepLinkingUrl, browserUrl, token);

		public Task OpenApp(string appScheme, string deepLinkingUrl, string browserUrl, CancellationToken token)
		{
			return _dispatcher.DispatchAsync(async () =>
			{
				var supportsUri = await _launcher.CanOpenAsync(appScheme);

				if (supportsUri)
					await _launcher.OpenAsync(deepLinkingUrl).WaitAsync(token);
				else
					await OpenBrowser(browserUrl, token);
			}).WaitAsync(token);
		}

		public Task SendEmail(string subject, string body, IEnumerable<string> recipients, CancellationToken token)
		{
			return _dispatcher.DispatchAsync(async () =>
			{
				var message = new EmailMessage
				{
					Subject = subject,
					Body = body,
					To = recipients.ToList()
				};

				try
				{
					await _email.ComposeAsync(message).WaitAsync(token).ConfigureAwait(false);
				}
				catch (FeatureNotSupportedException)
				{
					await DisplayAlert("No Email Client Found", "We'd love to hear your fedback!\nsupport@GitTrends.com", "OK", token).ConfigureAwait(false);
				}
			}).WaitAsync(token);
		}
	}
}