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
		Assert.Multiple(() =>
		{
			Assert.That(aboutViewModel.Watchers, Is.Null);
			Assert.That(aboutViewModel.Stars, Is.Null);
			Assert.That(aboutViewModel.Forks, Is.Null);

			Assert.That(aboutViewModel.InstalledLibraries, Is.Empty);
			Assert.That(aboutViewModel.GitTrendsContributors, Is.Empty);
		});
	}

	[Test]
	public async Task VerifyStatisticsTest()
	{
		//Arrange
		await ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>().Initialize(CancellationToken.None).ConfigureAwait(false);

		var aboutViewModel = ServiceCollection.ServiceProvider.GetRequiredService<AboutViewModel>();

		//Act

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(aboutViewModel.Watchers, Is.Not.Null);
			Assert.That(aboutViewModel.Stars, Is.Not.Null);
			Assert.That(aboutViewModel.Forks, Is.Not.Null);
			Assert.That(aboutViewModel.GitTrendsContributors, Is.Not.Empty);

			Assert.That(aboutViewModel.Watchers, Is.GreaterThan(0));
			Assert.That(aboutViewModel.Stars, Is.GreaterThan(0));
			Assert.That(aboutViewModel.Forks, Is.GreaterThan(0));
			Assert.That(aboutViewModel.GitTrendsContributors, Is.Not.Empty);
		});
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
		Assert.That(aboutViewModel.InstalledLibraries, Is.Not.Empty);
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
		var openedBrowserUri = await openAsyncExecutedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didBrowserOpen);
			Assert.That(gitTrendsStatisticsService.GitHubUri, Is.Not.Null);
			Assert.That(openedBrowserUri, Is.EqualTo(gitTrendsStatisticsService.GitHubUri));
		});

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
		var openedBrowserUri = await openAsyncExecutedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didBrowserOpen);
			Assert.That(gitTrendsStatisticsService.GitHubUri, Is.Not.Null);
			Assert.That(openedBrowserUri, Is.EqualTo(new Uri(gitTrendsStatisticsService.GitHubUri + "/issues/new?template=feature_request.md")));
		});

		void HandleOpenAsyncExecuted(object? sender, Uri e)
		{
			MockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;

			didBrowserOpen = true;
			openAsyncExecutedTCS.SetResult(e);
		}
	}
}