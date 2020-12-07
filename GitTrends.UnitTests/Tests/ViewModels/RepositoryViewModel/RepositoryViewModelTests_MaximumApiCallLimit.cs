using System.Collections.Generic;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    abstract class RepositoryViewModelTests_MaximumApiCallLimit : BaseTest
    {
        protected async Task ExecutePullToRefreshCommandTestMaximumApiLimitTest()
        {
            //Arrange
            PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
            IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;

            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
            string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();
            RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

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
            Assert.IsEmpty(visibleRepositoryList_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true), emptyDataViewTitle_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.MaximumApiLimit, true), emptyDataViewTitle_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true), emptyDataViewDescription_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.MaximumApiLimit, true), emptyDataViewDescription_Final);

            Assert.IsTrue(pullToRefreshFailedEventArgs is MaximumApiRequestsReachedEventArgs);

            void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
            {
                ReferringSitesViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
                pullToRefreshFailedTCS.SetResult(e);
            }
        }
    }
}
