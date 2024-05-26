namespace GitTrends.UnitTests;

class AboutViewModelTests : BaseTest
{
	[Test]
	public void VerifyUninitializedServicesTest()
	{
		//Arrange
		var aboutViewModel = ServiceCollection.ServiceProvider.GetRequiredService<AboutViewModel>();

		//Act

		//Assert
		Assert.IsNull(aboutViewModel.Watchers);
		Assert.IsNull(aboutViewModel.Stars);
		Assert.IsNull(aboutViewModel.Forks);

		Assert.IsEmpty(aboutViewModel.InstalledLibraries);
		Assert.IsEmpty(aboutViewModel.GitTrendsContributors);
	}

	[Test]
	public async Task VerifyStatisticsTest()
	{
		//Arrange
		await ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>().Initialize(CancellationToken.None).ConfigureAwait(false);

		var aboutViewModel = ServiceCollection.ServiceProvider.GetRequiredService<AboutViewModel>();

		//Act

		//Assert
		Assert.IsNotNull(aboutViewModel.Watchers);
		Assert.IsNotNull(aboutViewModel.Stars);
		Assert.IsNotNull(aboutViewModel.Forks);
		Assert.IsNotEmpty(aboutViewModel.GitTrendsContributors);

		Assert.Greater(aboutViewModel.Watchers, 0);
		Assert.Greater(aboutViewModel.Stars, 0);
		Assert.Greater(aboutViewModel.Forks, 0);
		Assert.Greater(aboutViewModel.GitTrendsContributors.Count, 0);
	}

	[Test]
	public async Task VerifyInstalledLibrariesTest()
	{
		//Arrange
		var librariesService = ServiceCollection.ServiceProvider.GetRequiredService<LibrariesService>();
		await librariesService.Initialize(CancellationToken.None).ConfigureAwait(false);

		var aboutViewModel = ServiceCollection.ServiceProvider.GetRequiredService<AboutViewModel>();

		//Act

		//Assert
		Assert.IsNotEmpty(aboutViewModel.InstalledLibraries);
		Assert.Greater(aboutViewModel.InstalledLibraries.Count, 0);
	}

	[Test]
	public async Task VerifyViewOnGitHubCommandTest()
	{
		//Arrange
		var didBrowserOpen = false;
		var openAsyncExecutedTCS = new TaskCompletionSource<Uri>();

		await ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>().Initialize(CancellationToken.None).ConfigureAwait(false);

		var aboutViewModel = ServiceCollection.ServiceProvider.GetRequiredService<AboutViewModel>();
		var gitTrendsStatisticsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>();

		MockBrowser.OpenAsyncExecuted += HandleOpenAsyncExecuted;

		//Act

		await aboutViewModel.ViewOnGitHubCommand.ExecuteAsync(null).ConfigureAwait(false);
		var openedBrowserUri = await openAsyncExecutedTCS.Task.ConfigureAwait(false);

		//Assert
		Assert.IsTrue(didBrowserOpen);
		Assert.IsNotNull(gitTrendsStatisticsService.GitHubUri);
		Assert.AreEqual(gitTrendsStatisticsService.GitHubUri, openedBrowserUri);

		void HandleOpenAsyncExecuted(object? sender, Uri e)
		{
			MockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;

			didBrowserOpen = true;
			openAsyncExecutedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task VerifyRequestFeatureCommandTest()
	{
		//Arrange
		var didBrowserOpen = false;
		var openAsyncExecutedTCS = new TaskCompletionSource<Uri>();

		await ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>().Initialize(CancellationToken.None).ConfigureAwait(false);

		var aboutViewModel = ServiceCollection.ServiceProvider.GetRequiredService<AboutViewModel>();
		var gitTrendsStatisticsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>();

		MockBrowser.OpenAsyncExecuted += HandleOpenAsyncExecuted;

		//Act

		await aboutViewModel.RequestFeatureCommand.ExecuteAsync(null).ConfigureAwait(false);
		var openedBrowserUri = await openAsyncExecutedTCS.Task.ConfigureAwait(false);

		//Assert
		Assert.IsTrue(didBrowserOpen);
		Assert.IsNotNull(gitTrendsStatisticsService.GitHubUri);
		Assert.AreEqual(new Uri(gitTrendsStatisticsService.GitHubUri + "/issues/new?template=feature_request.md"), openedBrowserUri);

		void HandleOpenAsyncExecuted(object? sender, Uri e)
		{
			MockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;

			didBrowserOpen = true;
			openAsyncExecutedTCS.SetResult(e);
		}
	}
}