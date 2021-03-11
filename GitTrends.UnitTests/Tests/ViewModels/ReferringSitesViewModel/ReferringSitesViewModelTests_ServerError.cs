using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace GitTrends.UnitTests
{
    [NonParallelizable]
    class ReferringSitesViewModelTests_ServerError : BaseTest
    {
        [Test]
        public async Task PullToRefreshTest_ServerError()
        {
            //Arrange
            PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
            string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
            bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
            IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();
            ReferringSitesViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

            //Act
            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
            isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
            mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
            emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

            var refreshCommandTask = referringSitesViewModel.RefreshCommand.ExecuteAsync((GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, $"https://github.com/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", CancellationToken.None));

            isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
            mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

            await refreshCommandTask.ConfigureAwait(false);

            emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
            isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
            mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
            emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

            pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.ConfigureAwait(false);

            //Asset
            Assert.IsFalse(isEmptyDataViewEnabled_Initial);
            Assert.IsFalse(isEmptyDataViewEnabled_DuringRefresh);
            Assert.True(isEmptyDataViewEnabled_Final);

            Assert.IsEmpty(mobileReferringSites_Initial);
            Assert.IsEmpty(mobileReferringSites_DuringRefresh);
            Assert.IsEmpty(mobileReferringSites_Final);

            Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Error), emptyDataViewTitle_Final);
            Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized), emptyDataViewTitle_Initial);

            Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Error), emptyDataViewDescription_Final);
            Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized), emptyDataViewDescription_Initial);

            Assert.IsTrue(pullToRefreshFailedEventArgs is ErrorPullToRefreshEventArgs);

            void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
            {
                ReferringSitesViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
                pullToRefreshFailedTCS.SetResult(e);
            }
        }

        protected override void InitializeServiceCollection()
        {
            var gitHubApiV3Client = RefitExtensions.For<IGitHubApiV3>(CreateMaximumApiLimitHttpClient(GitHubConstants.GitHubRestApiUrl));
            var gitHubGraphQLCLient = RefitExtensions.For<IGitHubGraphQLApi>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubGraphQLApi));
            var azureFunctionsClient = RefitExtensions.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl));

            ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
        }

        static HttpClient CreateMaximumApiLimitHttpClient(string url)
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
