using GitTrends.Common;

namespace GitTrends.UnitTests;

class WelcomeViewModelTests : BaseTest
{
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

		var welcomeViewModel = ServiceCollection.ServiceProvider.GetRequiredService<WelcomeViewModel>();

		//Act
		isAuthenticating_BeforeCommand = welcomeViewModel.IsAuthenticating;
		isDemoButtonVisible_BeforeCommand = welcomeViewModel.IsDemoButtonVisible;

		var connectToGitHubButtonCommandTask = welcomeViewModel.HandleConnectToGitHubButtonCommand.ExecuteAsync((CancellationToken.None, null));
		isAuthenticating_DuringCommand = welcomeViewModel.IsAuthenticating;
		isDemoButtonVisible_DuringCommand = welcomeViewModel.IsDemoButtonVisible;

		await connectToGitHubButtonCommandTask.ConfigureAwait(false);
		var openedUri = await openAsyncExecutedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		openedUrl = openedUri.AbsoluteUri;

		isAuthenticating_AfterCommand = welcomeViewModel.IsAuthenticating;
		isDemoButtonVisible_AfterCommand = welcomeViewModel.IsDemoButtonVisible;

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
	public async Task DemoButtonCommandTest()
	{
		//Arrange
		bool isDemoButtonVisible_Initial, isDemoButtonVisible_Final;

		var welcomeViewModel = ServiceCollection.ServiceProvider.GetRequiredService<WelcomeViewModel>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		isDemoButtonVisible_Initial = welcomeViewModel.IsDemoButtonVisible;

		await welcomeViewModel.HandleDemoButtonTappedCommand.ExecuteAsync(null).ConfigureAwait(false);

		isDemoButtonVisible_Final = welcomeViewModel.IsDemoButtonVisible;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(gitHubUserService.IsDemoUser);

			Assert.That(isDemoButtonVisible_Initial);
			Assert.That(isDemoButtonVisible_Final, Is.False);
		});
	}
}