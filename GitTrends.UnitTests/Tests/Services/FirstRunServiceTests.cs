
namespace GitTrends.UnitTests;

class FirstRunServiceTests : BaseTest
{
	[Test, Ignore("ToDo")]
	public Task FirstRunServiceTest_AuthorizeSessionCompleted()
	{
		throw new NotImplementedException();
	}

	[Test]
	public async Task FirstRunServiceTest_DemoUserActivated()
	{
		//Arrange
		bool isFirstRun_Initial, isFirstRun_Final;
		var activateDemoUserTCS = new TaskCompletionSource();

		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;

		var firstRunService = ServiceCollection.ServiceProvider.GetRequiredService<FirstRunService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();


		//Act
		isFirstRun_Initial = firstRunService.IsFirstRun;

		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		await activateDemoUserTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		isFirstRun_Final = firstRunService.IsFirstRun;

		Assert.Multiple(() =>
		{
			Assert.That(isFirstRun_Initial, Is.True);
			Assert.That(isFirstRun_Final, Is.False);
		});

		async void HandleDemoUserActivated(object? sender, EventArgs e)
		{
			GitHubAuthenticationService.DemoUserActivated -= HandleDemoUserActivated;

			await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			activateDemoUserTCS.SetResult();
		}
	}
}