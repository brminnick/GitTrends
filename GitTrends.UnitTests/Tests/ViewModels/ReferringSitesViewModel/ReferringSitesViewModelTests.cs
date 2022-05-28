using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
	[NonParallelizable]
	class ReferringSitesViewModelTests : BaseTest
	{
		[Test]
		public async Task PullToRefreshTest_UnauthenticatedUser()
		{
			//Arrange
			string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
			string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
			bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
			IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

			bool didPullToRefreshFailedFire = false;
			var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

			var mockGitTrendsRepository = new Repository(GitHubConstants.GitTrendsRepoName, "", 0, GitHubConstants.GitTrendsRepoOwner, AuthenticatedGitHubUserAvatarUrl, 0, 0, 0,
				$"https://github.com/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			ReferringSitesViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

			var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();

			//Act
			emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
			mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
			isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
			emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

			var refreshCommandTask = referringSitesViewModel.ExecuteRefreshCommand.ExecuteAsync((mockGitTrendsRepository, CancellationToken.None));

			isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
			mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

			await refreshCommandTask.ConfigureAwait(false);
			var pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.ConfigureAwait(false);

			emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
			mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
			isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
			emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

			//Assert
			Assert.IsTrue(didPullToRefreshFailedFire);
			Assert.IsTrue(pullToRefreshFailedEventArgs is MaximumApiRequestsReachedEventArgs || pullToRefreshFailedEventArgs is ErrorPullToRefreshEventArgs);

			Assert.IsFalse(isEmptyDataViewEnabled_Initial);
			Assert.IsFalse(isEmptyDataViewEnabled_DuringRefresh);
			Assert.IsTrue(isEmptyDataViewEnabled_Final);

			Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized), emptyDataViewTitle_Initial);
			Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Error), emptyDataViewTitle_Final);

			Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized), emptyDataViewDescription_Initial);
			Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Error), emptyDataViewDescription_Final);

			Assert.IsEmpty(mobileReferringSites_Initial);
			Assert.IsEmpty(mobileReferringSites_DuringRefresh);
			Assert.IsEmpty(mobileReferringSites_Final);


			void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
			{
				ReferringSitesViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;

				didPullToRefreshFailedFire = true;
				pullToRefreshFailedTCS.SetResult(e);
			}
		}

		[Test]
		public async Task PullToRefreshTest_AuthenticatedUser()
		{
			//Arrange
			string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
			string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;
			DateTimeOffset currentTime_Initial, currentTime_Final;
			bool isEmptyDataViewEnabled_Initial, isEmptyDataViewEnabled_DuringRefresh, isEmptyDataViewEnabled_Final;
			IReadOnlyList<MobileReferringSiteModel> mobileReferringSites_Initial, mobileReferringSites_DuringRefresh, mobileReferringSites_Final;

			var mockGitTrendsRepository = new Repository(GitHubConstants.GitTrendsRepoName, "", 0, GitHubConstants.GitTrendsRepoOwner, AuthenticatedGitHubUserAvatarUrl, 0, 0, 0,
				$"https://github.com/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var referringSitesViewModel = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesViewModel>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			//Act
			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			currentTime_Initial = DateTimeOffset.UtcNow;
			emptyDataViewTitle_Initial = referringSitesViewModel.EmptyDataViewTitle;
			isEmptyDataViewEnabled_Initial = referringSitesViewModel.IsEmptyDataViewEnabled;
			mobileReferringSites_Initial = referringSitesViewModel.MobileReferringSitesList;
			emptyDataViewDescription_Initial = referringSitesViewModel.EmptyDataViewDescription;

			var refreshCommandTask = referringSitesViewModel.ExecuteRefreshCommand.ExecuteAsync((mockGitTrendsRepository, CancellationToken.None));

			isEmptyDataViewEnabled_DuringRefresh = referringSitesViewModel.IsEmptyDataViewEnabled;
			mobileReferringSites_DuringRefresh = referringSitesViewModel.MobileReferringSitesList;

			await refreshCommandTask.ConfigureAwait(false);

			emptyDataViewTitle_Final = referringSitesViewModel.EmptyDataViewTitle;
			isEmptyDataViewEnabled_Final = referringSitesViewModel.IsEmptyDataViewEnabled;
			mobileReferringSites_Final = referringSitesViewModel.MobileReferringSitesList;
			emptyDataViewDescription_Final = referringSitesViewModel.EmptyDataViewDescription;

			currentTime_Final = DateTimeOffset.UtcNow;

			//Asset
			Assert.IsFalse(isEmptyDataViewEnabled_Initial);
			Assert.IsFalse(isEmptyDataViewEnabled_DuringRefresh);
			Assert.True(isEmptyDataViewEnabled_Final);

			Assert.IsEmpty(mobileReferringSites_Initial);
			Assert.IsEmpty(mobileReferringSites_DuringRefresh);
			Assert.IsNotEmpty(mobileReferringSites_Final);

			Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Succeeded), emptyDataViewTitle_Final);
			Assert.AreEqual(EmptyDataViewService.GetReferringSitesTitleText(RefreshState.Uninitialized), emptyDataViewTitle_Initial);

			Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Succeeded), emptyDataViewDescription_Final);
			Assert.AreEqual(EmptyDataViewService.GetReferringSitesDescriptionText(RefreshState.Uninitialized), emptyDataViewDescription_Initial);

			foreach (var referringSite in mobileReferringSites_Final)
			{
				Assert.Less(currentTime_Initial, referringSite.DownloadedAt);
				Assert.Greater(currentTime_Final, referringSite.DownloadedAt);
			}
		}
	}
}