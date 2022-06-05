using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Forms;

namespace GitTrends.UnitTests
{
	class TrendsViewModelTests : BaseTest
	{
		const int _timeoutInMilliseconds = 10000;

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser_OldData()
		{
			//Arrange
			Repository repository_OldData, repository_RepositorySavedToDatabaseResult;

			TaskCompletionSource<Repository> repositorySavedToDatabaseTCS = new();

			TrendsViewModel.RepositorySavedToDatabase += HandleRepositorySavedToDatabase;

			IReadOnlyList<DailyStarsModel> dailyStarsList_Initial, dailyStarsList_Final;
			IReadOnlyList<DailyViewsModel> dailyViewsList_Initial, dailyViewsList_Final;
			IReadOnlyList<DailyClonesModel> dailyClonesList_Initial, dailyClonesList_Final;

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			var repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None);
			await foreach (var completedReposiory in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(new[] { repository }, CancellationToken.None).ConfigureAwait(false))
			{
				repository = completedReposiory;
			}

			repository_OldData = repository with { DataDownloadedAt = DateTimeOffset.UtcNow.AddDays(-1) };

			//Act
			dailyStarsList_Initial = trendsViewModel.DailyStarsList;
			dailyViewsList_Initial = trendsViewModel.DailyViewsList;
			dailyClonesList_Initial = trendsViewModel.DailyClonesList;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository_OldData, CancellationToken.None)).ConfigureAwait(false);
			repository_RepositorySavedToDatabaseResult = await repositorySavedToDatabaseTCS.Task.ConfigureAwait(false);

			dailyStarsList_Final = trendsViewModel.DailyStarsList;
			dailyViewsList_Final = trendsViewModel.DailyViewsList;
			dailyClonesList_Final = trendsViewModel.DailyClonesList;

			//Assert
			Assert.IsNotEmpty(repository_OldData.DailyClonesList ?? throw new NullReferenceException());
			Assert.IsNotEmpty(repository_OldData.DailyViewsList ?? throw new NullReferenceException());
			Assert.IsNotEmpty(repository_OldData.StarredAt ?? throw new NullReferenceException());

			Assert.IsEmpty(dailyViewsList_Initial);
			Assert.IsEmpty(dailyClonesList_Initial);
			Assert.IsEmpty(dailyStarsList_Initial);

			Assert.IsNotEmpty(dailyViewsList_Final);
			Assert.IsNotEmpty(dailyClonesList_Final);
			Assert.IsNotEmpty(dailyStarsList_Final);

			Assert.AreEqual(repository_RepositorySavedToDatabaseResult.StarCount, dailyStarsList_Final.Select(x => x.TotalStars).Distinct().Count());

			for (int i = 0; i < dailyViewsList_Final.Count; i++)
			{
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyViewsList?[i].LocalDay, dailyViewsList_Final[i].LocalDay);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyViewsList?[i].TotalViews, dailyViewsList_Final[i].TotalViews);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyViewsList?[i].TotalUniqueViews, dailyViewsList_Final[i].TotalUniqueViews);
			}

			for (int i = 0; i < dailyClonesList_Final.Count; i++)
			{
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyClonesList?[i].LocalDay, dailyClonesList_Final[i].LocalDay);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyClonesList?[i].TotalClones, dailyClonesList_Final[i].TotalClones);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyClonesList?[i].TotalUniqueClones, dailyClonesList_Final[i].TotalUniqueClones);
			}

			void HandleRepositorySavedToDatabase(object? sender, Repository e)
			{
				TrendsViewModel.RepositorySavedToDatabase -= HandleRepositorySavedToDatabase;
				repositorySavedToDatabaseTCS.SetResult(e);
			}
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser_NoData()
		{
			//Arrange
			Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);
			Repository repository_RepositorySavedToDatabaseResult;

			TaskCompletionSource<Repository> repositorySavedToDatabaseTCS = new();

			TrendsViewModel.RepositorySavedToDatabase += HandleRepositorySavedToDatabase;

			IReadOnlyList<DailyStarsModel> dailyStarsList_Initial, dailyStarsList_Final;
			IReadOnlyList<DailyViewsModel> dailyViewsList_Initial, dailyViewsList_Final;
			IReadOnlyList<DailyClonesModel> dailyClonesList_Initial, dailyClonesList_Final;

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			dailyStarsList_Initial = trendsViewModel.DailyStarsList;
			dailyViewsList_Initial = trendsViewModel.DailyViewsList;
			dailyClonesList_Initial = trendsViewModel.DailyClonesList;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository_Initial, CancellationToken.None)).ConfigureAwait(false);
			repository_RepositorySavedToDatabaseResult = await repositorySavedToDatabaseTCS.Task.ConfigureAwait(false);

			dailyStarsList_Final = trendsViewModel.DailyStarsList;
			dailyViewsList_Final = trendsViewModel.DailyViewsList;
			dailyClonesList_Final = trendsViewModel.DailyClonesList;

			//Assert
			Assert.IsNull(repository_Initial.DailyClonesList);
			Assert.IsNull(repository_Initial.DailyViewsList);
			Assert.IsNull(repository_Initial.StarredAt);

			Assert.IsEmpty(dailyViewsList_Initial);
			Assert.IsEmpty(dailyClonesList_Initial);
			Assert.IsEmpty(dailyStarsList_Initial);

			Assert.IsNotEmpty(dailyViewsList_Final);
			Assert.IsNotEmpty(dailyClonesList_Final);
			Assert.IsNotEmpty(dailyStarsList_Final);

			Assert.AreEqual(repository_RepositorySavedToDatabaseResult.StarredAt?.Count, dailyStarsList_Final.Select(x => x.TotalStars).Distinct().Count());

			for (int i = 0; i < dailyViewsList_Final.Count; i++)
			{
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyViewsList?[i].LocalDay, dailyViewsList_Final[i].LocalDay);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyViewsList?[i].TotalViews, dailyViewsList_Final[i].TotalViews);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyViewsList?[i].TotalUniqueViews, dailyViewsList_Final[i].TotalUniqueViews);
			}

			for (int i = 0; i < dailyClonesList_Final.Count; i++)
			{
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyClonesList?[i].LocalDay, dailyClonesList_Final[i].LocalDay);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyClonesList?[i].TotalClones, dailyClonesList_Final[i].TotalClones);
				Assert.AreEqual(repository_RepositorySavedToDatabaseResult.DailyClonesList?[i].TotalUniqueClones, dailyClonesList_Final[i].TotalUniqueClones);
			}

			void HandleRepositorySavedToDatabase(object? sender, Repository e)
			{
				TrendsViewModel.RepositorySavedToDatabase -= HandleRepositorySavedToDatabase;
				repositorySavedToDatabaseTCS.SetResult(e);
			}
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser_NoData_FetchingStarsDataInBackground()
		{
			//Arrange
			bool wasTryScheduleRetryRepositoriesStarsSuccessful;
			Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act

			wasTryScheduleRetryRepositoriesStarsSuccessful = backgroundFetchService.TryScheduleRetryRepositoriesStars(repository_Initial);

			await FetchDataCommandTest_AuthenticatedUser_NoData();

			var isTryScheduleRetryRepositoriesStarsRunningAfterTest = backgroundFetchService.QueuedJobs.Any(x => x == backgroundFetchService.GetRetryRepositoriesStarsIdentifier(repository_Initial));

			//Assert
			Assert.IsTrue(wasTryScheduleRetryRepositoriesStarsSuccessful);
			Assert.IsFalse(isTryScheduleRetryRepositoriesStarsRunningAfterTest);
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser_IncompleteOriginalStarsData()
		{
			//Arrange
			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();
			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			var repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);
			repository = repository with { StarredAt = new List<DateTimeOffset> { new DateTimeOffset(2008, 02, 08, 0, 0, 0, TimeSpan.Zero) } };

			await repositoryDatabase.SaveRepository(repository);

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None));

			//Assert
			Assert.AreEqual(repository.StarCount, trendsViewModel.TotalStars);
			Assert.AreEqual(repository.StarCount, trendsViewModel.DailyStarsList.Max(x => x.TotalStars));
			Assert.AreEqual(repository.StarCount.ToAbbreviatedText(), trendsViewModel.StarsStatisticsText);
			Assert.AreEqual(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Succeeded, trendsViewModel.TotalStars), trendsViewModel.StarsHeaderMessageText);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewImage(RefreshState.Succeeded, trendsViewModel.TotalStars), ((FileImageSource?)trendsViewModel.StarsEmptyDataViewImage)?.File);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Succeeded, trendsViewModel.TotalStars), trendsViewModel.StarsEmptyDataViewTitleText);

			if (trendsViewModel.DailyStarsList[^1].TotalStars == trendsViewModel.DailyStarsList[^2].TotalStars)
				Assert.AreEqual(repository.StarCount, trendsViewModel.DailyStarsList.Count - 1);
			else
				Assert.AreEqual(repository.StarCount, trendsViewModel.DailyStarsList.Count);
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser_MissingStarsData()
		{
			//Arrange
			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();
			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			var repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);
			await repositoryDatabase.SaveRepository(repository);

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None));

			//Assert
			Assert.AreEqual(repository.StarCount, trendsViewModel.TotalStars);
			Assert.AreEqual(repository.StarCount, trendsViewModel.DailyStarsList.Max(x => x.TotalStars));
			Assert.AreEqual(repository.StarCount.ToAbbreviatedText(), trendsViewModel.StarsStatisticsText);
			Assert.AreEqual(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Succeeded, trendsViewModel.TotalStars), trendsViewModel.StarsHeaderMessageText);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewImage(RefreshState.Succeeded, trendsViewModel.TotalStars), ((FileImageSource?)trendsViewModel.StarsEmptyDataViewImage)?.File);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Succeeded, trendsViewModel.TotalStars), trendsViewModel.StarsEmptyDataViewTitleText);

			if (trendsViewModel.DailyStarsList[^1].TotalStars == trendsViewModel.DailyStarsList[^2].TotalStars)
				Assert.AreEqual(repository.StarCount, trendsViewModel.DailyStarsList.Count - 1);
			else
				Assert.AreEqual(repository.StarCount, trendsViewModel.DailyStarsList.Count);
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser_TokenCancelled()
		{
			//Arrange
			Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var cancellationTokenSource = new CancellationTokenSource();

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();
			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			var fetchDataCommandTask = trendsViewModel.FetchDataCommand.ExecuteAsync((repository_Initial, cancellationTokenSource.Token));

			cancellationTokenSource.Cancel();

			await fetchDataCommandTask.ConfigureAwait(false);

			//Assert
			Assert.AreEqual(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Error, trendsViewModel.TotalStars), trendsViewModel.StarsHeaderMessageText);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewImage(RefreshState.Error, trendsViewModel.TotalStars), ((FileImageSource?)trendsViewModel.StarsEmptyDataViewImage)?.File);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Error, trendsViewModel.TotalStars), trendsViewModel.StarsEmptyDataViewTitleText);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewDescriptionText(RefreshState.Error, trendsViewModel.TotalStars), trendsViewModel.StarsEmptyDataViewDescriptionText);
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser_NoData_FetchingViewsClonesStarsDataInBackground()
		{
			//Arrange
			bool wasTryScheduleRetryRepositoriesViewsClonesStarsSuccessful;
			Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN);

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			//Act
			wasTryScheduleRetryRepositoriesViewsClonesStarsSuccessful = backgroundFetchService.TryScheduleRetryRepositoriesViewsClonesStars(repository_Initial);

			await FetchDataCommandTest_AuthenticatedUser_NoData();

			var isTryScheduleRetryRepositoriesViewsClonesStarsRunningAfterTest = backgroundFetchService.QueuedJobs.Any(x => x == backgroundFetchService.GetRetryRepositoriesViewsClonesStarsIdentifier(repository_Initial));

			//Assert
			Assert.IsTrue(wasTryScheduleRetryRepositoriesViewsClonesStarsSuccessful);
			Assert.IsFalse(isTryScheduleRetryRepositoriesViewsClonesStarsRunningAfterTest);
		}

		[TestCase(true)]
		[TestCase(false)]
		[Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_AuthenticatedUser(bool shouldIncludeViewsClonesData)
		{
			//Arrange
			Repository repository;

			double dailyViewsClonesMinValue_Initial, dailyViewsClonesMinValue_Final;
			double dailyViewsClonesMaxValue_Initial, dailyViewsClonesMaxValue_Final;

			double minDailyStarsValue_Initial, minDailyStarsValue_Final;
			double maxDailyStarsValue_Initial, maxDailyStarsValue_Final;

			string viewsClonesEmptyDataViewTitleText_Initial, viewsClonesEmptyDataViewTitleText_Final;
			string starsEmptyDataViewTitleText_Initial, starsEmptyDataViewTitleText_Final;
			string starsHeaderMessageText_Initial, starsHeaderMessageText_Final;

			bool isViewsClonesChartVisible_Initial, isViewsClonesChartVisible_Final;
			bool isStarsChartVisible_Initial, isStarsChartVisible_Final;

			bool isFetchingData_Initial, isFetchingData_Final;

			bool isViewsClonesEmptyDataViewVisible_Initial, isViewsClonesEmptyDataViewVisible_Final;
			bool isStarsEmptyDataViewVisible_Initial, isStarsEmptyDataViewVisible_Final;

			DateTime minViewsClonesDate_Initial, minViewsClonesDate_Final;
			DateTime maxViewsClonesDate_Initial, maxViewsClonesDate_Final;

			DateTime minDailyStarsDate_Initial, minDailyStarsDate_Final;
			DateTime maxDailyStarsDate_Initial, maxDailyStarsDate_Final;

			IReadOnlyList<DailyStarsModel> dailyStarsList_Initial, dailyStarsList_Final;
			IReadOnlyList<DailyViewsModel> dailyViewsList_Initial, dailyViewsList_Final;
			IReadOnlyList<DailyClonesModel> dailyClonesList_Initial, dailyClonesList_Final;

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
			var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
			var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();


			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

			repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None);

			if (shouldIncludeViewsClonesData)
			{
				await foreach (var completedReposiory in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(new[] { repository }, CancellationToken.None).ConfigureAwait(false))
				{
					repository = completedReposiory;
				}
			}

			//Act
			isFetchingData_Initial = trendsViewModel.IsFetchingViewsClonesData;
			dailyStarsList_Initial = trendsViewModel.DailyStarsList;
			dailyViewsList_Initial = trendsViewModel.DailyViewsList;
			dailyClonesList_Initial = trendsViewModel.DailyClonesList;
			maxDailyStarsDate_Initial = trendsViewModel.MaxDailyStarsDate;
			minDailyStarsDate_Initial = trendsViewModel.MinDailyStarsDate;
			minDailyStarsValue_Initial = trendsViewModel.MinDailyStarsValue;
			maxDailyStarsValue_Initial = trendsViewModel.MaxDailyStarsValue;
			minViewsClonesDate_Initial = trendsViewModel.MinViewsClonesDate;
			maxViewsClonesDate_Initial = trendsViewModel.MaxViewsClonesDate;
			isStarsChartVisible_Initial = trendsViewModel.IsStarsChartVisible;
			starsHeaderMessageText_Initial = trendsViewModel.StarsHeaderMessageText;
			dailyViewsClonesMinValue_Initial = trendsViewModel.DailyViewsClonesMinValue;
			dailyViewsClonesMaxValue_Initial = trendsViewModel.DailyViewsClonesMaxValue;
			isViewsClonesChartVisible_Initial = trendsViewModel.IsViewsClonesChartVisible;
			starsEmptyDataViewTitleText_Initial = trendsViewModel.StarsEmptyDataViewTitleText;
			isStarsEmptyDataViewVisible_Initial = trendsViewModel.IsStarsEmptyDataViewVisible;
			viewsClonesEmptyDataViewTitleText_Initial = trendsViewModel.ViewsClonesEmptyDataViewTitleText;
			isViewsClonesEmptyDataViewVisible_Initial = trendsViewModel.IsViewsClonesEmptyDataViewVisible;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

			isFetchingData_Final = trendsViewModel.IsFetchingViewsClonesData;
			dailyStarsList_Final = trendsViewModel.DailyStarsList;
			dailyViewsList_Final = trendsViewModel.DailyViewsList;
			dailyClonesList_Final = trendsViewModel.DailyClonesList;
			maxDailyStarsDate_Final = trendsViewModel.MaxDailyStarsDate;
			minDailyStarsDate_Final = trendsViewModel.MinDailyStarsDate;
			minDailyStarsValue_Final = trendsViewModel.MinDailyStarsValue;
			maxDailyStarsValue_Final = trendsViewModel.MaxDailyStarsValue;
			isStarsChartVisible_Final = trendsViewModel.IsStarsChartVisible;
			minViewsClonesDate_Final = trendsViewModel.MinViewsClonesDate;
			maxViewsClonesDate_Final = trendsViewModel.MaxViewsClonesDate;
			starsHeaderMessageText_Final = trendsViewModel.StarsHeaderMessageText;
			dailyViewsClonesMinValue_Final = trendsViewModel.DailyViewsClonesMinValue;
			dailyViewsClonesMaxValue_Final = trendsViewModel.DailyViewsClonesMaxValue;
			isViewsClonesChartVisible_Final = trendsViewModel.IsViewsClonesChartVisible;
			starsEmptyDataViewTitleText_Final = trendsViewModel.StarsEmptyDataViewTitleText;
			isStarsEmptyDataViewVisible_Final = trendsViewModel.IsStarsEmptyDataViewVisible;
			viewsClonesEmptyDataViewTitleText_Final = trendsViewModel.ViewsClonesEmptyDataViewTitleText;
			isViewsClonesEmptyDataViewVisible_Final = trendsViewModel.IsViewsClonesEmptyDataViewVisible;

			//Assert
			Assert.IsEmpty(dailyViewsList_Initial);
			Assert.IsEmpty(dailyClonesList_Initial);
			Assert.IsEmpty(dailyStarsList_Initial);

			Assert.IsNotEmpty(dailyViewsList_Final);
			Assert.IsNotEmpty(dailyClonesList_Final);
			Assert.IsNotEmpty(dailyStarsList_Final);

			Assert.IsTrue(isFetchingData_Initial);
			Assert.IsFalse(isFetchingData_Final);

			Assert.IsFalse(isViewsClonesEmptyDataViewVisible_Initial);
			Assert.IsFalse(isViewsClonesEmptyDataViewVisible_Final);

			Assert.IsFalse(isStarsEmptyDataViewVisible_Initial);
			Assert.IsFalse(isStarsEmptyDataViewVisible_Final);

			Assert.IsFalse(isViewsClonesChartVisible_Initial);
			Assert.True(isViewsClonesChartVisible_Final);

			Assert.IsFalse(isStarsChartVisible_Initial);
			Assert.True(isStarsChartVisible_Final);

			Assert.AreEqual(TrendsViewModel.MinimumChartHeight, dailyViewsClonesMaxValue_Initial);
			Assert.GreaterOrEqual(dailyViewsClonesMaxValue_Final, dailyViewsClonesMaxValue_Initial);

			Assert.AreEqual(TrendsViewModel.MinimumChartHeight, maxDailyStarsValue_Initial);
			Assert.GreaterOrEqual(maxDailyStarsValue_Final, maxDailyStarsValue_Initial);

			Assert.AreEqual(0, dailyViewsClonesMinValue_Initial);
			Assert.AreEqual(dailyViewsClonesMinValue_Final, dailyViewsClonesMinValue_Initial);

			Assert.AreEqual(0, minDailyStarsValue_Initial);
			Assert.AreEqual(minDailyStarsValue_Final, minDailyStarsValue_Initial);

			Assert.AreEqual(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13)).Date, minViewsClonesDate_Initial.Date);
			Assert.LessOrEqual(minViewsClonesDate_Final.Date, minViewsClonesDate_Initial.Date);

			Assert.AreEqual(DateTimeOffset.UtcNow.Date, maxViewsClonesDate_Initial.Date);
			Assert.LessOrEqual(maxViewsClonesDate_Final.Date, maxViewsClonesDate_Initial.Date);

			Assert.LessOrEqual(minDailyStarsDate_Final.Date, minDailyStarsDate_Initial.Date);
			Assert.LessOrEqual(maxDailyStarsDate_Final.Date, maxDailyStarsDate_Initial.Date);

			Assert.AreEqual(EmptyDataViewService.GetViewsClonesTitleText(RefreshState.Uninitialized), viewsClonesEmptyDataViewTitleText_Initial);
			Assert.AreEqual(EmptyDataViewService.GetViewsClonesTitleText(RefreshState.Succeeded), viewsClonesEmptyDataViewTitleText_Final);

			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Uninitialized, trendsViewModel.TotalStars), starsEmptyDataViewTitleText_Initial);
			Assert.AreEqual(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Succeeded, trendsViewModel.TotalStars), starsEmptyDataViewTitleText_Final);

			Assert.AreEqual(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Uninitialized, default), starsHeaderMessageText_Initial);
			Assert.AreEqual(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Succeeded, trendsViewModel.TotalStars), starsHeaderMessageText_Final);
		}

		[TestCase(true)]
		[TestCase(false)]
		[Timeout(_timeoutInMilliseconds)]
		public async Task FetchDataCommandTest_DemoUser(bool shouldCreateViewsClonesData)
		{
			//Arrange
			IReadOnlyList<DailyStarsModel> dailyStarsList_Initial, dailyStarsList_Final;
			IReadOnlyList<DailyViewsModel> dailyViewsList_Initial, dailyViewsList_Final;
			IReadOnlyList<DailyClonesModel> dailyClonesList_Initial, dailyClonesList_Final;

			Repository repository = CreateRepository(shouldCreateViewsClonesData);

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

			//Act
			dailyStarsList_Initial = trendsViewModel.DailyStarsList;
			dailyViewsList_Initial = trendsViewModel.DailyViewsList;
			dailyClonesList_Initial = trendsViewModel.DailyClonesList;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

			dailyStarsList_Final = trendsViewModel.DailyStarsList;
			dailyViewsList_Final = trendsViewModel.DailyViewsList;
			dailyClonesList_Final = trendsViewModel.DailyClonesList;

			//Assret
			Assert.IsEmpty(dailyStarsList_Initial);
			Assert.IsEmpty(dailyViewsList_Initial);
			Assert.IsEmpty(dailyClonesList_Initial);

			Assert.IsNotEmpty(dailyStarsList_Final);
			Assert.IsNotEmpty(dailyViewsList_Final);
			Assert.IsNotEmpty(dailyClonesList_Final);
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task UniqueClonesCardTappedCommandTest()
		{
			//Arrange
			bool isUniqueClonesSeriesVisible_Initial, isUniqueClonesSeriesVisible_AfterFetchDataCommand, isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand;
			string uniqueClonesStatisticsText_Initial, uniqueClonesStatisticsText_AfterFetchDataCommand, uniqueClonesStatisticsText_AfterViewsCardTappedCommand;

			Repository repository = CreateRepository();

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

			//Act
			isUniqueClonesSeriesVisible_Initial = trendsViewModel.IsUniqueClonesSeriesVisible;
			uniqueClonesStatisticsText_Initial = trendsViewModel.UniqueClonesStatisticsText;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

			isUniqueClonesSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsUniqueClonesSeriesVisible;
			uniqueClonesStatisticsText_AfterFetchDataCommand = trendsViewModel.UniqueClonesStatisticsText;

			trendsViewModel.UniqueClonesCardTappedCommand.Execute(null);

			isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsUniqueClonesSeriesVisible;
			uniqueClonesStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.UniqueClonesStatisticsText;

			//Assert
			Assert.AreEqual(string.Empty, uniqueClonesStatisticsText_Initial);
			Assert.AreEqual(repository.TotalUniqueClones.ToAbbreviatedText(), uniqueClonesStatisticsText_AfterFetchDataCommand);
			Assert.AreEqual(repository.TotalUniqueClones.ToAbbreviatedText(), uniqueClonesStatisticsText_AfterViewsCardTappedCommand);

			Assert.IsTrue(isUniqueClonesSeriesVisible_Initial);
			Assert.IsTrue(isUniqueClonesSeriesVisible_AfterFetchDataCommand);
			Assert.IsFalse(isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand);
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task ClonesCardTappedCommandTest()
		{
			//Arrange
			bool isClonesSeriesVisible_Initial, isClonesSeriesVisible_AfterFetchDataCommand, isClonesSeriesVisible_AfterViewsCardTappedCommand;
			string clonesStatisticsText_Initial, clonesStatisticsText_AfterFetchDataCommand, clonesStatisticsText_AfterViewsCardTappedCommand;

			Repository repository = CreateRepository();

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

			//Act
			isClonesSeriesVisible_Initial = trendsViewModel.IsClonesSeriesVisible;
			clonesStatisticsText_Initial = trendsViewModel.ClonesStatisticsText;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

			isClonesSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsClonesSeriesVisible;
			clonesStatisticsText_AfterFetchDataCommand = trendsViewModel.ClonesStatisticsText;

			trendsViewModel.ClonesCardTappedCommand.Execute(null);

			isClonesSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsClonesSeriesVisible;
			clonesStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.ClonesStatisticsText;

			//Assert
			Assert.AreEqual(string.Empty, clonesStatisticsText_Initial);
			Assert.AreEqual(repository.TotalClones.ToAbbreviatedText(), clonesStatisticsText_AfterFetchDataCommand);
			Assert.AreEqual(repository.TotalClones.ToAbbreviatedText(), clonesStatisticsText_AfterViewsCardTappedCommand);

			Assert.IsTrue(isClonesSeriesVisible_Initial);
			Assert.IsTrue(isClonesSeriesVisible_AfterFetchDataCommand);
			Assert.IsFalse(isClonesSeriesVisible_AfterViewsCardTappedCommand);
		}


		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task UniqueViewsCardTappedCommandTest()
		{
			//Arrange
			bool isUniqueViewsSeriesVisible_Initial, isUniqueViewsSeriesVisible_AfterFetchDataCommand, isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand;
			string uniqueViewsStatisticsText_Initial, uniqueViewsStatisticsText_AfterFetchDataCommand, uniqueViewsStatisticsText_AfterViewsCardTappedCommand;

			Repository repository = CreateRepository();

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

			//Act
			isUniqueViewsSeriesVisible_Initial = trendsViewModel.IsUniqueViewsSeriesVisible;
			uniqueViewsStatisticsText_Initial = trendsViewModel.UniqueViewsStatisticsText;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

			isUniqueViewsSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsUniqueViewsSeriesVisible;
			uniqueViewsStatisticsText_AfterFetchDataCommand = trendsViewModel.UniqueViewsStatisticsText;

			trendsViewModel.UniqueViewsCardTappedCommand.Execute(null);

			isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsUniqueViewsSeriesVisible;
			uniqueViewsStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.UniqueViewsStatisticsText;

			//Assert
			Assert.AreEqual(string.Empty, uniqueViewsStatisticsText_Initial);
			Assert.AreEqual(repository.TotalUniqueViews.ToAbbreviatedText(), uniqueViewsStatisticsText_AfterFetchDataCommand);
			Assert.AreEqual(repository.TotalUniqueViews.ToAbbreviatedText(), uniqueViewsStatisticsText_AfterViewsCardTappedCommand);

			Assert.IsTrue(isUniqueViewsSeriesVisible_Initial);
			Assert.IsTrue(isUniqueViewsSeriesVisible_AfterFetchDataCommand);
			Assert.IsFalse(isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand);
		}

		[Test, Timeout(_timeoutInMilliseconds)]
		public async Task ViewsCardTappedCommandTest()
		{
			//Arrange
			bool isViewsSeriesVisible_Initial, isViewsSeriesVisible_AfterFetchDataCommand, isViewsSeriesVisible_AfterViewsCardTappedCommand;
			string viewsStatisticsText_Initial, viewsStatisticsText_AfterFetchDataCommand, viewsStatisticsText_AfterViewsCardTappedCommand;

			Repository repository = CreateRepository();

			var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

			var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
			await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

			//Act
			isViewsSeriesVisible_Initial = trendsViewModel.IsViewsSeriesVisible;
			viewsStatisticsText_Initial = trendsViewModel.ViewsStatisticsText;

			await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

			isViewsSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsViewsSeriesVisible;
			viewsStatisticsText_AfterFetchDataCommand = trendsViewModel.ViewsStatisticsText;

			trendsViewModel.ViewsCardTappedCommand.Execute(null);

			isViewsSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsViewsSeriesVisible;
			viewsStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.ViewsStatisticsText;

			//Assert
			Assert.AreEqual(string.Empty, viewsStatisticsText_Initial);
			Assert.AreEqual(repository.TotalViews.ToAbbreviatedText(), viewsStatisticsText_AfterFetchDataCommand);
			Assert.AreEqual(repository.TotalViews.ToAbbreviatedText(), viewsStatisticsText_AfterViewsCardTappedCommand);

			Assert.IsTrue(isViewsSeriesVisible_Initial);
			Assert.IsTrue(isViewsSeriesVisible_AfterFetchDataCommand);
			Assert.IsFalse(isViewsSeriesVisible_AfterViewsCardTappedCommand);
		}
	}
}