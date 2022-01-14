using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests
{
	[NonParallelizable]
	class RepositoryViewModelTests_ServerError : BaseTest
	{
		[Test]
		public async Task PullToRefreshCommandTest_ServerError()
		{
			//Arrange
			PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
			IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;
			IReadOnlyList<Repository> generatedRepositories = new[]
			{
				CreateRepository(),
				CreateRepository(),
				CreateRepository(),
				CreateRepository(),
				CreateRepository(),
			};

			string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
			string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
			var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();
			RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

			foreach (var repository in generatedRepositories)
				await repositoryDatabase.SaveRepository(repository).ConfigureAwait(false);

			generatedRepositories = await repositoryDatabase.GetRepositories().ConfigureAwait(false);

			//Act
			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
			visibleRepositoryList_Initial = repositoryViewModel.VisibleRepositoryList;
			emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

			await repositoryViewModel.PullToRefreshCommand.ExecuteAsync().ConfigureAwait(false);

			emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
			visibleRepositoryList_Final = repositoryViewModel.VisibleRepositoryList;
			emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

			pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.ConfigureAwait(false);

			//Assert
			Assert.IsEmpty(visibleRepositoryList_Initial);
			Assert.IsNotEmpty(visibleRepositoryList_Final);

			Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true), emptyDataViewTitle_Initial);
			Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Error, false), emptyDataViewTitle_Final);

			Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true), emptyDataViewDescription_Initial);
			Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Error, false), emptyDataViewDescription_Final);

			Assert.IsInstanceOf<ErrorPullToRefreshEventArgs>(pullToRefreshFailedEventArgs);

			foreach (var visibleRepository in visibleRepositoryList_Final)
			{
				//Ensure visibleRepositoryList_Final matches the generatedRepositories
				var matchingRepository = generatedRepositories.Single(x => x.Url == visibleRepository.Url);

				Assert.AreEqual(visibleRepository.DailyClonesList?.Count, matchingRepository.DailyClonesList?.Count);
				Assert.AreEqual(visibleRepository.DailyViewsList?.Count, matchingRepository.DailyViewsList?.Count);
				Assert.AreEqual(visibleRepository.DataDownloadedAt, matchingRepository.DataDownloadedAt);
				Assert.AreEqual(visibleRepository.Description, matchingRepository.Description);
				Assert.AreEqual(visibleRepository.ForkCount, matchingRepository.ForkCount);
				Assert.AreEqual(visibleRepository.IsFavorite, matchingRepository.IsFavorite);
				Assert.AreEqual(visibleRepository.IsFork, matchingRepository.IsFork);
				Assert.AreEqual(visibleRepository.IssuesCount, matchingRepository.IssuesCount);
				Assert.AreEqual(visibleRepository.IsTrending, matchingRepository.IsTrending);
				Assert.AreEqual(visibleRepository.Name, matchingRepository.Name);
				Assert.AreEqual(visibleRepository.OwnerAvatarUrl, matchingRepository.OwnerAvatarUrl);
				Assert.AreEqual(visibleRepository.OwnerLogin, matchingRepository.OwnerLogin);
				Assert.AreEqual(visibleRepository.StarCount, matchingRepository.StarCount);
				Assert.AreEqual(visibleRepository.StarredAt?.Count, matchingRepository.StarredAt?.Count);
				Assert.AreEqual(visibleRepository.TotalClones, matchingRepository.TotalClones);
				Assert.AreEqual(visibleRepository.TotalUniqueClones, matchingRepository.TotalUniqueClones);
				Assert.AreEqual(visibleRepository.TotalUniqueViews, matchingRepository.TotalUniqueViews);
				Assert.AreEqual(visibleRepository.TotalViews, matchingRepository.TotalViews);
				Assert.AreEqual(visibleRepository.Url, matchingRepository.Url);
			}

			void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
			{
				RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
				pullToRefreshFailedTCS.SetResult(e);
			}
		}

		protected override void InitializeServiceCollection()
		{
			var gitHubApiV3Client = RefitExtensions.For<IGitHubApiV3>(CreateServerErrorHttpClient(GitHubConstants.GitHubRestApiUrl));
			var gitHubGraphQLCLient = RefitExtensions.For<IGitHubGraphQLApi>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubGraphQLApi));
			var azureFunctionsClient = RefitExtensions.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl));

			ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
		}

		static HttpClient CreateServerErrorHttpClient(string url)
		{
			var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

			var httpMessageHandler = new MockHttpMessageHandler();
			httpMessageHandler.When($"{url}/*").Respond(request => responseMessage);

			var httpClient = httpMessageHandler.ToHttpClient();
			httpClient.BaseAddress = new Uri(url);

			return httpClient;
		}
	}
}