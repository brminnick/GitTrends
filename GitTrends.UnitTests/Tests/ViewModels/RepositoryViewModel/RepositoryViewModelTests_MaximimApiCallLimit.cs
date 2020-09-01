using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Refit;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests
{
    class RepositoryViewModelTests_MaximimApiCallLimit : BaseTest
    {
        [Test]
        public async Task PullToRefreshCommandTest_MaximumApiLimit()
        {
            //Arrange
            DateTimeOffset beforePullToRefresh, afterPullToRefresh;
            IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;

            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
            string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            //Act
            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            beforePullToRefresh = DateTimeOffset.UtcNow;
            emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
            visibleRepositoryList_Initial = repositoryViewModel.VisibleRepositoryList;
            emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

            await repositoryViewModel.PullToRefreshCommand.ExecuteAsync().ConfigureAwait(false);

            afterPullToRefresh = DateTimeOffset.UtcNow;
            emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
            visibleRepositoryList_Final = repositoryViewModel.VisibleRepositoryList;
            emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

            //Assert

            throw new NotImplementedException("Below Assert Statements Copy/Pasted from RepositoryViewModelTests");
            Assert.IsEmpty(visibleRepositoryList_Initial);
            Assert.IsNotEmpty(visibleRepositoryList_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true), emptyDataViewTitle_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Succeeded, false), emptyDataViewTitle_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true), emptyDataViewDescription_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Succeeded, false), emptyDataViewDescription_Final);

            Assert.IsTrue(visibleRepositoryList_Final.Any(x => x.OwnerLogin is GitTrendsRepoOwner && x.Name is GitTrendsRepoName));

            foreach (var repository in visibleRepositoryList_Final)
            {
                Assert.Less(beforePullToRefresh, repository.DataDownloadedAt);
                Assert.Greater(afterPullToRefresh, repository.DataDownloadedAt);
            }
        }

        protected override void InitializeServiceCollection()
        {
            var gitHubApiV3Client = RestService.For<IGitHubApiV3>(CreateMaximumApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));
            var gitHubGraphQLCLient = RestService.For<IGitHubGraphQLApi>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubGraphQLApi));
            var azureFunctionsClient = RestService.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl));

            ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
        }

        static HttpClient CreateMaximumApiLimitHttpClient(string url)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Forbidden);
            responseMessage.Headers.Add(GitHubApiService.RateLimitRemainingHeader, "0");
            responseMessage.Headers.Add(GitHubApiService.RateLimitResetHeader, DateTimeOffset.UtcNow.AddMinutes(50).ToUnixTimeSeconds().ToString());

            var httpMessageHandler = new MockHttpMessageHandler();
            httpMessageHandler.When($"{url}/*").Respond(request => responseMessage);

            var httpClient = httpMessageHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri(url);

            return httpClient;
        }
    }
}
