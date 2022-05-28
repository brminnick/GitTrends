using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace GitTrends.UnitTests
{
	[NonParallelizable]
	abstract class RepositoryViewModelTests_AbuseLimit : BaseTest
	{
		protected async Task ExecutePullToRefreshCommandTestAbuseLimit()
		{
			//Arrange
			PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs;
			IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;

			int gitHubApiAbuseLimitCount_Initial, gitHubApiAbuseLimitCount_Final;

			string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
			string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

			RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

			//Act
			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			gitHubApiAbuseLimitCount_Initial = gitHubUserService.GitHubApiAbuseLimitCount;

			emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
			visibleRepositoryList_Initial = repositoryViewModel.VisibleRepositoryList;
			emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

			await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(null).ConfigureAwait(false);

			emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
			visibleRepositoryList_Final = repositoryViewModel.VisibleRepositoryList;
			emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

			pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.ConfigureAwait(false);

			gitHubApiAbuseLimitCount_Final = gitHubUserService.GitHubApiAbuseLimitCount;

			//Assert
			Assert.IsEmpty(visibleRepositoryList_Initial);

			Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true), emptyDataViewTitle_Initial);
			Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.AbuseLimit, !visibleRepositoryList_Final.Any()), emptyDataViewTitle_Final);

			Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true), emptyDataViewDescription_Initial);
			Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.AbuseLimit, !visibleRepositoryList_Final.Any()), emptyDataViewDescription_Final);

			Assert.IsInstanceOf<AbuseLimitPullToRefreshEventArgs>(pullToRefreshFailedEventArgs);
			Assert.AreEqual(0, gitHubApiAbuseLimitCount_Initial);
			Assert.Greater(gitHubApiAbuseLimitCount_Final, 0);

			void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
			{
				RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
				pullToRefreshFailedTCS.SetResult(e);
			}
		}
	}
}