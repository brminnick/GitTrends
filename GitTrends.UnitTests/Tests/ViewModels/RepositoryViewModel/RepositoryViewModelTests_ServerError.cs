using System.Net;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests;

[NonParallelizable]
class RepositoryViewModelTests_ServerError : BaseTest
{
	[Test]
	public async Task PullToRefreshCommandTest_ServerError()
	{
		//Arrange
		PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
		IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;
		IReadOnlyList<Repository> generatedRepositories =
		[
			CreateRepository(),
			CreateRepository(),
			CreateRepository(),
			CreateRepository(),
			CreateRepository(),
		];

		string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
		string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var backgroundFetchService = (ExtendedBackgroundFetchService)ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();

		var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();
		RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		foreach (var repository in generatedRepositories)
			await repositoryDatabase.SaveRepository(repository, TestCancellationTokenSource.Token).ConfigureAwait(false);

		generatedRepositories = await repositoryDatabase.GetRepositories(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Initial = new List<Repository>(repositoryViewModel.VisibleRepositoryList);
		emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(null).ConfigureAwait(false);
		backgroundFetchService.CancelAllJobs();

		emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Final = new List<Repository>(repositoryViewModel.VisibleRepositoryList);
		emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

		pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(visibleRepositoryList_Initial, Is.Empty);
			Assert.That(visibleRepositoryList_Final, Is.Not.Empty);

			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Error, false)));

			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Error, false)));

			Assert.That(pullToRefreshFailedEventArgs, Is.InstanceOf<ErrorPullToRefreshEventArgs>());


			foreach (var visibleRepository in visibleRepositoryList_Final)
			{
				//Ensure visibleRepositoryList_Final matches the generatedRepositories
				var matchingRepository = generatedRepositories.Single(x => x.Url == visibleRepository.Url);

				Assert.That(matchingRepository.DailyClonesList?.Count, Is.EqualTo(visibleRepository.DailyClonesList?.Count));
				Assert.That(matchingRepository.DailyViewsList?.Count, Is.EqualTo(visibleRepository.DailyViewsList?.Count));
				Assert.That(matchingRepository.DataDownloadedAt, Is.EqualTo(visibleRepository.DataDownloadedAt));
				Assert.That(matchingRepository.Description, Is.EqualTo(visibleRepository.Description));
				Assert.That(matchingRepository.ForkCount, Is.EqualTo(visibleRepository.ForkCount));
				Assert.That(matchingRepository.IsFavorite, Is.EqualTo(visibleRepository.IsFavorite));
				Assert.That(matchingRepository.IsFork, Is.EqualTo(visibleRepository.IsFork));
				Assert.That(matchingRepository.IssuesCount, Is.EqualTo(visibleRepository.IssuesCount));
				Assert.That(matchingRepository.IsTrending, Is.EqualTo(visibleRepository.IsTrending));
				Assert.That(matchingRepository.Name, Is.EqualTo(visibleRepository.Name));
				Assert.That(matchingRepository.OwnerAvatarUrl, Is.EqualTo(visibleRepository.OwnerAvatarUrl));
				Assert.That(matchingRepository.OwnerLogin, Is.EqualTo(visibleRepository.OwnerLogin));
				Assert.That(matchingRepository.StarCount, Is.EqualTo(visibleRepository.StarCount));
				Assert.That(matchingRepository.StarredAt?.Count, Is.EqualTo(visibleRepository.StarredAt?.Count));
				Assert.That(matchingRepository.TotalClones, Is.EqualTo(visibleRepository.TotalClones));
				Assert.That(matchingRepository.TotalUniqueClones, Is.EqualTo(visibleRepository.TotalUniqueClones));
				Assert.That(matchingRepository.TotalUniqueViews, Is.EqualTo(visibleRepository.TotalUniqueViews));
				Assert.That(matchingRepository.TotalViews, Is.EqualTo(visibleRepository.TotalViews));
				Assert.That(matchingRepository.Url, Is.EqualTo(visibleRepository.Url));
			}
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			pullToRefreshFailedTCS.SetResult(e);
		}
	}

	protected override void InitializeServiceCollection()
	{
		var handler = new HttpClientHandler
		{
			AutomaticDecompression = GetDecompressionMethods()
		};

		var gitHubApiV3Client = RestService.For<IGitHubApiV3>(CreateServerErrorHttpClient(GitHubConstants.GitHubRestApiUrl));

		var gitHubGraphQLCLient = RestService.For<IGitHubGraphQLApi>(new HttpClient(handler)
		{
			BaseAddress = new(GitHubConstants.GitHubGraphQLApi)
		});

		var azureFunctionsClient = RestService.For<IAzureFunctionsApi>(new HttpClient(handler)
		{
			BaseAddress = new(AzureConstants.AzureFunctionsApiUrl)
		});

		ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
	}

	static HttpClient CreateServerErrorHttpClient(string url)
	{
		var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

		var httpMessageHandler = new MockHttpMessageHandler();
		httpMessageHandler.When($"{url}/*").Respond(_ => responseMessage);

		var httpClient = httpMessageHandler.ToHttpClient();
		httpClient.BaseAddress = new Uri(url);

		return httpClient;
	}
}