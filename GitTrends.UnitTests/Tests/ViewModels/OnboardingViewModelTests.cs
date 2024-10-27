using GitTrends.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.UnitTests;

class OnboardingViewModelTests : BaseTest
{
	[Test]
	public async Task DemoButtonCommand_Skip()
	{
		//Arrange
		bool didSkipButtonTappedFire = false;
		var skipButtonTappedTCS = new TaskCompletionSource();

		var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();
		OnboardingViewModel.SkipButtonTapped += HandleSkipButtonTapped;

		//Act
		await onboardingViewModel.HandleDemoButtonTappedCommand.ExecuteAsync(OnboardingConstants.SkipText).ConfigureAwait(false);
		await skipButtonTappedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.That(didSkipButtonTappedFire);

		void HandleSkipButtonTapped(object? sender, EventArgs e)
		{
			OnboardingViewModel.SkipButtonTapped -= HandleSkipButtonTapped;

			didSkipButtonTappedFire = true;
			skipButtonTappedTCS.SetResult();
		}
	}

	[Test]
	public async Task DemoButtonCommand_TryDemo()
	{
		//Arrange
		var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		await onboardingViewModel.HandleDemoButtonTappedCommand.ExecuteAsync(OnboardingConstants.TryDemoText).ConfigureAwait(false);

		//Assert
		Assert.That(gitHubUserService.IsDemoUser);
	}

	[Test]
	public async Task ConnectToGitHubButtonCommandTest()
	{
		//Arrange
		string openedUrl;
		bool isAuthenticating_BeforeCommand, isAuthenticating_DuringCommand, isAuthenticating_AfterCommand;
		bool isDemoButtonVisible_BeforeCommand, isDemoButtonVisible_DuringCommand, isDemoButtonVisible_AfterCommand;

		bool didOpenAsyncFire = false;
		var openAsyncExecutedTCS = new TaskCompletionSource<Uri>();

		MockBrowser.OpenAsyncExecuted += HandleOpenAsyncExecuted;

		var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();
		var gitTrendsStatisticsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>();

		//Act
		await gitTrendsStatisticsService.Initialize(CancellationToken.None).ConfigureAwait(false);

		isAuthenticating_BeforeCommand = onboardingViewModel.IsAuthenticating;
		isDemoButtonVisible_BeforeCommand = onboardingViewModel.IsDemoButtonVisible;

		var connectToGitHubButtonCommandTask = onboardingViewModel.HandleConnectToGitHubButtonCommand.ExecuteAsync((CancellationToken.None, null));

		isAuthenticating_DuringCommand = onboardingViewModel.IsAuthenticating;
		isDemoButtonVisible_DuringCommand = onboardingViewModel.IsDemoButtonVisible;

		await connectToGitHubButtonCommandTask.ConfigureAwait(false);
		var openedUri = await openAsyncExecutedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		openedUrl = openedUri.AbsoluteUri;

		isAuthenticating_AfterCommand = onboardingViewModel.IsAuthenticating;
		isDemoButtonVisible_AfterCommand = onboardingViewModel.IsDemoButtonVisible;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didOpenAsyncFire);

			Assert.That(isAuthenticating_BeforeCommand, Is.False);
			Assert.That(isDemoButtonVisible_BeforeCommand);

			Assert.That(isAuthenticating_DuringCommand);
			Assert.That(isDemoButtonVisible_DuringCommand, Is.False);

			Assert.That(isAuthenticating_AfterCommand, Is.False);
			Assert.That(isDemoButtonVisible_AfterCommand);

			Assert.That(openedUrl, Does.Contain($"{GitHubConstants.GitHubBaseUrl}/login/oauth/authorize?client_id="));
			Assert.That(openedUrl, Does.Contain($"&scope={GitHubConstants.OAuthScope}&state="));
		});

		void HandleOpenAsyncExecuted(object? sender, Uri e)
		{
			MockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;
			didOpenAsyncFire = true;

			openAsyncExecutedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task EnableNotificationsButtonTappedTest()
	{
		//Arrange
		const string successSvg = "check.svg";
		const string bellSvg = "bell.svg";

		string notificationStatusSvgImageSource_Initial, notificationStatusSvgImageSource_Final;
		var onboardingViewModel = ServiceCollection.ServiceProvider.GetRequiredService<OnboardingViewModel>();

		//Act
		notificationStatusSvgImageSource_Initial = onboardingViewModel.NotificationStatusSvgImageSource;

		await onboardingViewModel.HandleEnableNotificationsButtonTappedCommand.ExecuteAsync(null).ConfigureAwait(false);

		notificationStatusSvgImageSource_Final = onboardingViewModel.NotificationStatusSvgImageSource;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(notificationStatusSvgImageSource_Initial, Is.EqualTo(bellSvg));
			Assert.That(notificationStatusSvgImageSource_Final, Is.EqualTo(successSvg));
		});
	}
}