using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public sealed class WelcomePage : BaseContentPage<WelcomeViewModel>, IDisposable
{
	const int _demoLabelFontSize = 16;

	readonly CancellationTokenSource _connectToGitHubCancellationTokenSource = new();
	readonly IAppInfo _appInfo;

	public WelcomePage(IAppInfo appInfo,
		IDeviceInfo deviceInfo,
		WelcomeViewModel welcomeViewModel,
		IAnalyticsService analyticsService)
		: base(welcomeViewModel, analyticsService, shouldUseSafeArea: true)
	{
		_appInfo = appInfo;

		Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);

		RemoveDynamicResource(BackgroundColorProperty);
		On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

		var pageBackgroundColor = Color.FromArgb(BaseTheme.LightTealColorHex);
		BackgroundColor = pageBackgroundColor;

		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

		var browserLaunchOptions = new BrowserLaunchOptions
		{
			PreferredToolbarColor = pageBackgroundColor.MultiplyAlpha(0.75f),
			PreferredControlColor = Colors.White,
			Flags = BrowserLaunchFlags.PresentAsFormSheet
		};

		Content = new Grid
		{
			Padding = 8,
			RowSpacing = 24,

			RowDefinitions = GridRowsColumns.Rows.Define(
				(Row.WelcomeLabel, Stars(2)),
				(Row.Image, Stars(4)),
				(Row.GitHubButton, Stars(2)),
				(Row.DemoButton, Stars(1)),
				(Row.VersionLabel, Stars(1))),

			Children =
			{
				new WelcomeLabel()
					.Row(Row.WelcomeLabel),

				new Image { Source = "WelcomeImage" }.Center()
					.Row(Row.Image),

				new GitHubButton(deviceInfo, WelcomePageAutomationIds.ConnectToGitHubButton, GitHubLoginButtonConstants.ConnectToGitHub).CenterHorizontal().Bottom()
					.Row(Row.GitHubButton)
					.Bind(GitHubButton.CommandProperty, 
						getter: static (WelcomeViewModel vm) => vm.HandleConnectToGitHubButtonCommand,
						mode: BindingMode.OneTime)
					.Invoke(button => button.CommandParameter = (_connectToGitHubCancellationTokenSource.Token, browserLaunchOptions)),

				new DemoLabel()
					.Row(Row.DemoButton),

				new ConnectToGitHubActivityIndicator()
					.Row(Row.DemoButton),

				new VersionNumberLabel(_appInfo)
					.Row(Row.VersionLabel)
			}
		};
	}

	enum Row { WelcomeLabel, Image, GitHubButton, DemoButton, VersionLabel }

	public void Dispose()
	{
		_connectToGitHubCancellationTokenSource.Dispose();
	}

	protected override void OnDisappearing()
	{
		_connectToGitHubCancellationTokenSource.Cancel();

		base.OnDisappearing();
	}

	async void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e)
	{
		if (e.IsSessionAuthorized)
			await PopPage();
	}

	async void HandleDemoUserActivated(object? sender, EventArgs e)
	{
		var minimumActivityIndicatorTime = Task.Delay(TimeSpan.FromMilliseconds(1500));

		await minimumActivityIndicatorTime;
		await PopPage();
	}

	Task PopPage() => Dispatcher.DispatchAsync(Navigation.PopModalAsync);

	sealed class ConnectToGitHubActivityIndicator : ActivityIndicator
	{
		public ConnectToGitHubActivityIndicator()
		{
			Color = Colors.White;

			AutomationId = WelcomePageAutomationIds.IsAuthenticatingActivityIndicator;

			HorizontalOptions = LayoutOptions.Center;
			VerticalOptions = LayoutOptions.Start;

			HeightRequest = WidthRequest = _demoLabelFontSize;

			this.Bind(IsVisibleProperty, 
				getter: static (WelcomeViewModel vm) => vm.IsAuthenticating);
			this.Bind(IsRunningProperty, 
				getter: static (WelcomeViewModel vm) => vm.IsAuthenticating);
		}
	}

	sealed class VersionNumberLabel : Label
	{
		public VersionNumberLabel(IAppInfo appInfo)
		{
			Text = $"v{appInfo.Version}";
			TextColor = Colors.White;

			FontSize = 12;
			FontFamily = FontFamilyConstants.RobotoBold;

			this.Center();

			HorizontalTextAlignment = TextAlignment.Center;
			VerticalTextAlignment = TextAlignment.End;
		}
	}

	sealed class DemoLabel : Label
	{
		public DemoLabel()
		{
			Text = WelcomePageConstants.TryDemo;
			TextColor = Colors.White;

			FontSize = _demoLabelFontSize;
			FontFamily = FontFamilyConstants.RobotoRegular;

			HorizontalOptions = LayoutOptions.Center;
			VerticalOptions = LayoutOptions.Start;

			HorizontalTextAlignment = TextAlignment.Center;
			VerticalTextAlignment = TextAlignment.Start;

			Opacity = 0.75;

			AutomationId = WelcomePageAutomationIds.DemoModeButton;

			this.BindTapGesture(nameof(WelcomeViewModel.HandleDemoButtonTappedCommand));
			this.Bind(IsVisibleProperty, nameof(WelcomeViewModel.IsDemoButtonVisible));
		}
	}

	sealed class WelcomeLabel : Label
	{
		public WelcomeLabel()
		{
			HorizontalTextAlignment = TextAlignment.Center;
			VerticalTextAlignment = TextAlignment.Center;

			TextColor = Colors.White;
			FormattedText = new FormattedString
			{
				Spans =
				{
					new Span
					{
						FontSize = 32,
						FontFamily = FontFamilyConstants.RobotoBold,
						Text = WelcomePageConstants.WelcomeToGitTrends,
					},
					new Span
					{
						Text = "\n"
					},
					new Span
					{
						FontSize = 16,
						FontFamily = FontFamilyConstants.RobotoRegular,
						Text = WelcomePageConstants.MonitorYourRepos
					}
				}
			};
		}
	}
}