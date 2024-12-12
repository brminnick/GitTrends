using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Resources;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public sealed class SplashScreenPage : BaseContentPage, IDisposable
{
	readonly IEnumerator<string> _statusMessageEnumerator = new List<string>
	{
		SplashScreenPageConstants.Initializing,
		SplashScreenPageConstants.ConnectingToServers,
		SplashScreenPageConstants.Initializing,
		SplashScreenPageConstants.ConnectingToServers,
		SplashScreenPageConstants.Initializing,
		SplashScreenPageConstants.ConnectingToServers,
		SplashScreenPageConstants.StillWorkingOnIt,
		SplashScreenPageConstants.LetsTryItLikeThis,
		SplashScreenPageConstants.MaybeThis,
		SplashScreenPageConstants.AnotherTry,
		SplashScreenPageConstants.ItShouldntTakeThisLong,
		SplashScreenPageConstants.AreYouSureInternetConnectionIsGood
	}.GetEnumerator();

	readonly Label _loadingLabel;
	readonly Image _gitTrendsImage;
	readonly IDeviceDisplay _deviceDisplay;
	readonly FirstRunService _firstRunService;
	readonly AppInitializationService _appInitializationService;

	CancellationTokenSource _animationCancellationToken = new();

	public SplashScreenPage(
		IDeviceDisplay deviceDisplay,
		FirstRunService firstRunService,
		IAnalyticsService analyticsService,
		AppInitializationService appInitializationService)
		: base(analyticsService)
	{
		Shell.SetNavBarIsVisible(this, false);
		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.GitTrendsImageBackgroundColor));

		_deviceDisplay = deviceDisplay;
		_firstRunService = firstRunService;
		_appInitializationService = appInitializationService;

		_statusMessageEnumerator.MoveNext();

		Content = new Grid
		{
			RowDefinitions = Rows.Define(
				(Row.Image, Star),
				(Row.Text, Auto),
				(Row.BottomPadding, 50)),

			Children =
			{
				new LoadingLabel(deviceDisplay).Center().Assign(out _loadingLabel)
					.Row(Row.Text),

				new GitTrendsImage().Center().Assign(out _gitTrendsImage)
					.RowSpan(All<Row>()),
			}
		};
	}

	enum Row { Image, Text, BottomPadding }

	public void Dispose() => _animationCancellationToken.Dispose();

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await ChangeLabelText(_statusMessageEnumerator.Current);

		_animationCancellationToken = new CancellationTokenSource();

		//Fade the Image Opacity to 1. Work around for https://github.com/xamarin/Xamarin.Forms/issues/8073
		var fadeImageTask = _gitTrendsImage.FadeTo(1, 1000, Easing.CubicIn);
		var pulseImageTask = PulseImage();

		//Slide status label into screen
		await _loadingLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
		await _loadingLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

		//Wait for Image to reach an opacity of 1
		await Task.WhenAll(fadeImageTask, pulseImageTask);
		await Task.Delay(100);

		AppInitializationService.InitializationCompleted += HandleInitializationCompleted;

		if (_appInitializationService.IsInitializationComplete)
		{
			AppInitializationService.InitializationCompleted -= HandleInitializationCompleted;

			await HandleInitializationCompleted(true);
		}
		else
		{
			Animate(_animationCancellationToken.Token);
		}
	}

	async void Animate(CancellationToken pulseCancellationToken) => await Dispatcher.DispatchAsync(async () =>
	{
		while (!pulseCancellationToken.IsCancellationRequested)
		{
			var pulseImageTask = PulseImage();
			await Task.Delay(TimeSpan.FromMilliseconds(400), pulseCancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);

			//Label leaves the screen
			await _loadingLabel.TranslateTo(10, 0, 100, Easing.CubicInOut);
			await _loadingLabel.TranslateTo(-_deviceDisplay.MainDisplayInfo.Width / 2, 0, 250, Easing.CubicIn);

			//Move the label to the other side of the screen
			_loadingLabel.TranslationX = _deviceDisplay.MainDisplayInfo.Width / 2;

			//Update Status Label Text
			if (!_statusMessageEnumerator.MoveNext())
			{
				_statusMessageEnumerator.Reset();
				_statusMessageEnumerator.MoveNext();
			}
			await ChangeLabelText(_statusMessageEnumerator.Current);

			//Label reappears on the screen
			await _loadingLabel.TranslateTo(-10, 0, 250, Easing.CubicOut);
			await _loadingLabel.TranslateTo(0, 0, 250, Easing.CubicOut);

			await pulseImageTask;
			await Task.Delay(TimeSpan.FromMilliseconds(250), pulseCancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
		}
	});

	Task PulseImage() => Dispatcher.DispatchAsync(async () =>
	{
		//Image crouches down
		await _gitTrendsImage.ScaleTo(0.95, 100, Easing.CubicInOut);
		await Task.Delay(TimeSpan.FromMilliseconds(50));

		//Image jumps
		await _gitTrendsImage.ScaleTo(1.25, 250, Easing.CubicOut);

		//Image crashes back to the screen
		await _gitTrendsImage.ScaleTo(1, 500, Easing.BounceOut);
	});

	Task ChangeLabelText(string text) => ChangeLabelText(new FormattedString
	{
		Spans =
		{
			new Span
			{
				Text = text,
				FontFamily = FontFamilyConstants.RobotoRegular
			}
		}
	});

	Task ChangeLabelText(string title, string body) => ChangeLabelText(new FormattedString
	{
		Spans =
		{
			new Span
			{
				Text = title,
				FontSize = 16,
				FontFamily = FontFamilyConstants.RobotoBold
			},
			new Span
			{
				Text = "\n" + body,
				FontFamily = FontFamilyConstants.RobotoRegular
			}
		}
	});

	Task ChangeLabelText(FormattedString formattedString) => Dispatcher.DispatchAsync(async () =>
	{
		await _loadingLabel.FadeTo(0, 250, Easing.CubicOut);

		_loadingLabel.Text = string.Empty;
		_loadingLabel.FormattedText = formattedString;

		await _loadingLabel.FadeTo(1, 250, Easing.CubicIn);
	});

	async void HandleInitializationCompleted(object? sender, InitializationCompleteEventArgs e)
	{
		AppInitializationService.InitializationCompleted -= HandleInitializationCompleted;
		await HandleInitializationCompleted(e.IsInitializationSuccessful).ConfigureAwait(false);
	}

	async Task HandleInitializationCompleted(bool isInitializationSuccessful)
	{
		await _animationCancellationToken.CancelAsync();

		if (isInitializationSuccessful)
		{
#if DEBUG
			await ChangeLabelText(SplashScreenPageConstants.PreviewMode, SplashScreenPageConstants.WarningsMayAppear);
			//Display Text
			await Task.Delay(TimeSpan.FromMilliseconds(500));
#else
                await ChangeLabelText("Let's go!");
#endif
			await NavigateToNextPage();
		}
		else
		{
			await ChangeLabelText(SplashScreenPageConstants.InitializationFailed, $"\n{SplashScreenPageConstants.EnsureInternetConnectionAndLatestVersion}");

			AnalyticsService.Track("Initialization Failed");
		}

		Task NavigateToNextPage()
		{
			return Dispatcher.DispatchAsync(async () =>
			{
				//Explode & Fade Everything
				var explodeImageTask = Task.WhenAll(Content.ScaleTo(100, 250, Easing.CubicOut), Content.FadeTo(0, 250, Easing.CubicIn));
				BackgroundColor = AppResources.GetResource<Color>(nameof(BaseTheme.PageBackgroundColor));

				await explodeImageTask;

				await Shell.Current.GoToAsync(AppShell.GetPageRoute<RepositoryPage>());
			});
		}
	}

	sealed class GitTrendsImage : Image
	{
		public GitTrendsImage()
		{
			Opacity = 0;
			Aspect = Aspect.AspectFit;
			AutomationId = SplashScreenPageAutomationIds.GitTrendsImage;

			this.Center().DynamicResource(SourceProperty, nameof(BaseTheme.GitTrendsImageSource));
		}
	}

	sealed class LoadingLabel : Label
	{
		public LoadingLabel(IDeviceDisplay deviceDisplay)
		{
			//Begin with Label off of the screen
			TranslationX = deviceDisplay.MainDisplayInfo.Width / 2;

			Margin = new Thickness(10, 0);
			HorizontalTextAlignment = TextAlignment.Center;
			AutomationId = SplashScreenPageAutomationIds.StatusLabel;
			this.DynamicResource(TextColorProperty, nameof(BaseTheme.SplashScreenStatusColor));
		}
	}
}