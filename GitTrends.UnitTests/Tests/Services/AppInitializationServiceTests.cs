namespace GitTrends.UnitTests;

class AppInitializationServiceTests : BaseTest
{
	[Test]
	public async Task InitializeAppCommandTest()
	{
		//Arrange
		bool didInitializationCompleteFire = false;
		var initializeAppCommandTCS = new TaskCompletionSource<InitializationCompleteEventArgs>();

		AppInitializationService.InitializationCompleted += HandleInitializationComplete;

		var appInitializationService = ServiceCollection.ServiceProvider.GetRequiredService<AppInitializationService>();

		//Assert
		Assert.That(appInitializationService.IsInitializationComplete, Is.False);

		//Act
		var isInitializationSuccessful = await appInitializationService.InitializeApp(CancellationToken.None).ConfigureAwait(false);
		var initializationCompleteEventArgs = await initializeAppCommandTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(isInitializationSuccessful);
			Assert.That(didInitializationCompleteFire);
			Assert.That(appInitializationService.IsInitializationComplete);
			Assert.That(initializationCompleteEventArgs.IsInitializationSuccessful);
		});
		Assert.That(initializationCompleteEventArgs.IsInitializationSuccessful, Is.EqualTo(isInitializationSuccessful));

		void HandleInitializationComplete(object? sender, InitializationCompleteEventArgs e)
		{
			AppInitializationService.InitializationCompleted -= HandleInitializationComplete;

			didInitializationCompleteFire = true;
			initializeAppCommandTCS.SetResult(e);
		}
	}
}