using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

class TrendsViewModelTests : BaseTest
{
	const int _timeoutInMilliseconds = 10000;

	[Test, CancelAfter(_timeoutInMilliseconds)]
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

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		var repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);
		await foreach (var completedReposiory in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData([repository], CancellationToken.None).ConfigureAwait(false))
		{
			repository = completedReposiory;
		}

		repository_OldData = repository with
		{
			DataDownloadedAt = DateTimeOffset.UtcNow.AddDays(-1)
		};

		//Act
		dailyStarsList_Initial = trendsViewModel.DailyStarsList;
		dailyViewsList_Initial = trendsViewModel.DailyViewsList;
		dailyClonesList_Initial = trendsViewModel.DailyClonesList;

		await trendsViewModel.FetchData(repository_OldData, CancellationToken.None).ConfigureAwait(false);
		repository_RepositorySavedToDatabaseResult = await repositorySavedToDatabaseTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		dailyStarsList_Final = trendsViewModel.DailyStarsList;
		dailyViewsList_Final = trendsViewModel.DailyViewsList;
		dailyClonesList_Final = trendsViewModel.DailyClonesList;

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(repository_OldData.DailyClonesList ?? throw new NullReferenceException(), Is.Not.Empty);
			Assert.That(repository_OldData.DailyViewsList ?? throw new NullReferenceException(), Is.Not.Empty);
			Assert.That(repository_OldData.StarredAt ?? throw new NullReferenceException(), Is.Not.Empty);

			Assert.That(dailyViewsList_Initial, Is.Empty);
			Assert.That(dailyClonesList_Initial, Is.Empty);
			Assert.That(dailyStarsList_Initial, Is.Empty);

			Assert.That(dailyViewsList_Final, Is.Not.Empty);
			Assert.That(dailyClonesList_Final, Is.Not.Empty);
			Assert.That(dailyStarsList_Final, Is.Not.Empty);

			Assert.That(dailyStarsList_Final.Select(static x => x.TotalStars).Distinct().Count(), Is.EqualTo(repository_RepositorySavedToDatabaseResult.StarCount));

			foreach (var dailyViewsModel_Final in dailyViewsList_Final)
			{
				var dailyViewsModel_SavedToDatabase = repository_RepositorySavedToDatabaseResult
					.DailyViewsList?.FirstOrDefault(x => x.Day == dailyViewsModel_Final.Day);

				if (dailyViewsModel_SavedToDatabase is null)
					continue;

				Assert.That(dailyViewsModel_Final.LocalDay, Is.EqualTo(dailyViewsModel_SavedToDatabase.LocalDay));
				Assert.That(dailyViewsModel_Final.TotalViews, Is.EqualTo(dailyViewsModel_SavedToDatabase.TotalViews));
				Assert.That(dailyViewsModel_Final.TotalUniqueViews, Is.EqualTo(dailyViewsModel_SavedToDatabase.TotalUniqueViews));
			}

			foreach (var dailyClonesModel_Final in dailyClonesList_Final)
			{
				var dailyClonesModel_SavedToDatabase = repository_RepositorySavedToDatabaseResult
					.DailyClonesList?.FirstOrDefault(x => x.Day == dailyClonesModel_Final.Day);

				if (dailyClonesModel_SavedToDatabase is null)
					continue;

				Assert.That(dailyClonesModel_Final.LocalDay, Is.EqualTo(dailyClonesModel_SavedToDatabase.LocalDay));
				Assert.That(dailyClonesModel_Final.TotalClones, Is.EqualTo(dailyClonesModel_SavedToDatabase.TotalClones));
				Assert.That(dailyClonesModel_Final.TotalUniqueClones, Is.EqualTo(dailyClonesModel_SavedToDatabase.TotalUniqueClones));
			}
		});

		void HandleRepositorySavedToDatabase(object? sender, Repository e)
		{
			TrendsViewModel.RepositorySavedToDatabase -= HandleRepositorySavedToDatabase;
			repositorySavedToDatabaseTCS.SetResult(e);
		}
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task FetchDataCommandTest_AuthenticatedUser_NoData()
	{
		//Arrange
		Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);
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

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		dailyStarsList_Initial = trendsViewModel.DailyStarsList;
		dailyViewsList_Initial = trendsViewModel.DailyViewsList;
		dailyClonesList_Initial = trendsViewModel.DailyClonesList;

		await trendsViewModel.FetchData(repository_Initial, CancellationToken.None).ConfigureAwait(false);
		repository_RepositorySavedToDatabaseResult = await repositorySavedToDatabaseTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		dailyStarsList_Final = trendsViewModel.DailyStarsList;
		dailyViewsList_Final = trendsViewModel.DailyViewsList;
		dailyClonesList_Final = trendsViewModel.DailyClonesList;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repository_Initial.DailyClonesList, Is.Null);
			Assert.That(repository_Initial.DailyViewsList, Is.Null);

			Assert.That(repository_Initial.StarredAt, Is.Null);

			Assert.That(dailyViewsList_Initial, Is.Empty);
			Assert.That(dailyClonesList_Initial, Is.Empty);
			Assert.That(dailyStarsList_Initial, Is.Empty);

			Assert.That(dailyViewsList_Final, Is.Not.Empty);
			Assert.That(dailyClonesList_Final, Is.Not.Empty);
			Assert.That(dailyStarsList_Final, Is.Not.Empty);

			Assert.That(dailyStarsList_Final.Select(static x => x.TotalStars).Distinct().Count(), Is.EqualTo(repository_RepositorySavedToDatabaseResult.StarredAt?.Count));

			foreach (var dailyViewsModel_Final in dailyViewsList_Final)
			{
				var dailyViewsModel_SavedToDatabase = repository_RepositorySavedToDatabaseResult
					.DailyViewsList?.FirstOrDefault(x => x.Day == dailyViewsModel_Final.Day);

				if (dailyViewsModel_SavedToDatabase is null)
					continue;

				Assert.That(dailyViewsModel_Final.LocalDay, Is.EqualTo(dailyViewsModel_SavedToDatabase.LocalDay));
				Assert.That(dailyViewsModel_Final.TotalViews, Is.EqualTo(dailyViewsModel_SavedToDatabase.TotalViews));
				Assert.That(dailyViewsModel_SavedToDatabase.TotalUniqueViews, Is.EqualTo(dailyViewsModel_Final.TotalUniqueViews));
			}

			foreach (var dailyClonesModel_Final in dailyClonesList_Final)
			{
				var dailyClonesModel_SavedToDatabase = repository_RepositorySavedToDatabaseResult
					.DailyClonesList?.FirstOrDefault(x => x.Day == dailyClonesModel_Final.Day);

				if (dailyClonesModel_SavedToDatabase is null)
					continue;

				Assert.That(dailyClonesModel_Final.LocalDay, Is.EqualTo(dailyClonesModel_SavedToDatabase.LocalDay));
				Assert.That(dailyClonesModel_Final.TotalClones, Is.EqualTo(dailyClonesModel_SavedToDatabase.TotalClones));
				Assert.That(dailyClonesModel_Final.TotalUniqueClones, Is.EqualTo(dailyClonesModel_SavedToDatabase.TotalUniqueClones));
			}
		});

		void HandleRepositorySavedToDatabase(object? sender, Repository e)
		{
			TrendsViewModel.RepositorySavedToDatabase -= HandleRepositorySavedToDatabase;
			repositorySavedToDatabaseTCS.SetResult(e);
		}
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task FetchDataCommandTest_AuthenticatedUser_NoData_FetchingStarsDataInBackground()
	{
		//Arrange
		bool wasTryScheduleRetryRepositoriesStarsSuccessful;
		Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		wasTryScheduleRetryRepositoriesStarsSuccessful = backgroundFetchService.TryScheduleRetryRepositoriesStars(repository_Initial);

		await FetchDataCommandTest_AuthenticatedUser_NoData().ConfigureAwait(false);

		var isTryScheduleRetryRepositoriesStarsRunningAfterTest = backgroundFetchService.IsFetchingStarsInBackground(repository_Initial);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(wasTryScheduleRetryRepositoriesStarsSuccessful);
			Assert.That(isTryScheduleRetryRepositoriesStarsRunningAfterTest, Is.False);
		});
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task FetchDataCommandTest_AuthenticatedUser_IncompleteOriginalStarsData()
	{
		//Arrange
		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		var repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);
		repository = repository with
		{
			StarredAt = [new(2008, 02, 08, 0, 0, 0, TimeSpan.Zero)]
		};

		await repositoryDatabase.SaveRepository(repository, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(trendsViewModel.TotalStars, Is.EqualTo(repository.StarCount));
			Assert.That(trendsViewModel.DailyStarsList.Max(static x => x.TotalStars), Is.EqualTo(repository.StarCount));
			Assert.That(trendsViewModel.StarsStatisticsText, Is.EqualTo(repository.StarCount.ToAbbreviatedText()));
			Assert.That(trendsViewModel.StarsHeaderMessageText, Is.EqualTo(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Succeeded, trendsViewModel.TotalStars)));
			Assert.That(((FileImageSource?)trendsViewModel.StarsEmptyDataViewImage)?.File, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewImage(RefreshState.Succeeded, trendsViewModel.TotalStars)));
			Assert.That(trendsViewModel.StarsEmptyDataViewTitleText, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Succeeded, trendsViewModel.TotalStars)));

			if (Math.Abs(trendsViewModel.DailyStarsList[^1].TotalStars - trendsViewModel.DailyStarsList[^2].TotalStars) < double.Epsilon)
				Assert.That(trendsViewModel.DailyStarsList.Count - 1, Is.EqualTo(repository.StarCount));
			else
				Assert.That(trendsViewModel.DailyStarsList, Has.Count.EqualTo(repository.StarCount));
		});
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task FetchDataCommandTest_AuthenticatedUser_MissingStarsData()
	{
		//Arrange
		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		var repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);
		await repositoryDatabase.SaveRepository(repository, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(trendsViewModel.TotalStars, Is.EqualTo(repository.StarCount));
			Assert.That(trendsViewModel.DailyStarsList.Max(static x => x.TotalStars), Is.EqualTo(repository.StarCount));
			Assert.That(trendsViewModel.StarsStatisticsText, Is.EqualTo(repository.StarCount.ToAbbreviatedText()));
			Assert.That(trendsViewModel.StarsHeaderMessageText, Is.EqualTo(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Succeeded, trendsViewModel.TotalStars)));
			Assert.That(((FileImageSource?)trendsViewModel.StarsEmptyDataViewImage)?.File, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewImage(RefreshState.Succeeded, trendsViewModel.TotalStars)));
			Assert.That(trendsViewModel.StarsEmptyDataViewTitleText, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Succeeded, trendsViewModel.TotalStars)));

			if (Math.Abs(trendsViewModel.DailyStarsList[^1].TotalStars - trendsViewModel.DailyStarsList[^2].TotalStars) < double.Epsilon)
				Assert.That(trendsViewModel.DailyStarsList.Count - 1, Is.EqualTo(repository.StarCount));
			else
				Assert.That(trendsViewModel.DailyStarsList, Has.Count.EqualTo(repository.StarCount));
		});
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task FetchDataCommandTest_AuthenticatedUser_TokenCancelled()
	{
		//Arrange
		Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var cancellationTokenSource = new CancellationTokenSource();

		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		var fetchDataCommandTask = trendsViewModel.FetchData(repository_Initial, cancellationTokenSource.Token);

		await cancellationTokenSource.CancelAsync();

		//Assert
		Assert.Multiple(() =>
		{
			Assert.ThrowsAsync<TaskCanceledException>(() => fetchDataCommandTask);
			Assert.That(trendsViewModel.StarsHeaderMessageText, Is.EqualTo(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Error, trendsViewModel.TotalStars)));
			Assert.That(((FileImageSource?)trendsViewModel.StarsEmptyDataViewImage)?.File, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewImage(RefreshState.Error, trendsViewModel.TotalStars)));
			Assert.That(trendsViewModel.StarsEmptyDataViewTitleText, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Error, trendsViewModel.TotalStars)));
			Assert.That(trendsViewModel.StarsEmptyDataViewDescriptionText, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewDescriptionText(RefreshState.Error, trendsViewModel.TotalStars)));
		});
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task FetchDataCommandTest_AuthenticatedUser_NoData_FetchingViewsClonesStarsDataInBackground()
	{
		//Arrange
		bool wasTryScheduleRetryRepositoriesViewsClonesStarsSuccessful;
		Repository repository_Initial = new Repository(GitHubConstants.GitTrendsRepoName, string.Empty, 0, GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsAvatarUrl, 0, 0, 0, $"{GitHubConstants.GitHubBaseUrl}/{GitHubConstants.GitTrendsRepoOwner}/{GitHubConstants.GitTrendsRepoName}", false, DateTimeOffset.UtcNow, RepositoryPermission.ADMIN, false);

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		wasTryScheduleRetryRepositoriesViewsClonesStarsSuccessful = backgroundFetchService.TryScheduleRetryRepositoriesViewsClonesStars(repository_Initial);

		await FetchDataCommandTest_AuthenticatedUser_NoData().ConfigureAwait(false);

		var isTryScheduleRetryRepositoriesViewsClonesStarsRunningAfterTest = backgroundFetchService.IsFetchingStarsInBackground(repository_Initial);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(wasTryScheduleRetryRepositoriesViewsClonesStarsSuccessful, Is.True);
			Assert.That(isTryScheduleRetryRepositoriesViewsClonesStarsRunningAfterTest, Is.False);
		});
	}

	[TestCase(true)]
	[TestCase(false)]
	[CancelAfter(_timeoutInMilliseconds)]
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


		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		repository = await gitHubGraphQLApiService.GetRepository(GitHubConstants.GitTrendsRepoOwner, GitHubConstants.GitTrendsRepoName, CancellationToken.None).ConfigureAwait(false);

		if (shouldIncludeViewsClonesData)
		{
			await foreach (var completedReposiory in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData([repository], CancellationToken.None).ConfigureAwait(false))
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

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

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

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(dailyViewsList_Initial, Is.Empty);
			Assert.That(dailyClonesList_Initial, Is.Empty);
			Assert.That(dailyStarsList_Initial, Is.Empty);

			Assert.That(dailyViewsList_Final, Is.Not.Empty);
			Assert.That(dailyClonesList_Final, Is.Not.Empty);
			Assert.That(dailyStarsList_Final, Is.Not.Empty);

			Assert.That(isFetchingData_Initial);
			Assert.That(isFetchingData_Final, Is.False);

			Assert.That(isViewsClonesEmptyDataViewVisible_Initial, Is.False);
			Assert.That(isViewsClonesEmptyDataViewVisible_Final, Is.False);

			Assert.That(isStarsEmptyDataViewVisible_Initial, Is.False);
			Assert.That(isStarsEmptyDataViewVisible_Final, Is.False);

			Assert.That(isViewsClonesChartVisible_Initial, Is.False);
			Assert.That(isViewsClonesChartVisible_Final);

			Assert.That(isStarsChartVisible_Initial, Is.False);
			Assert.That(isStarsChartVisible_Final);

			Assert.That(dailyViewsClonesMaxValue_Initial, Is.EqualTo(TrendsViewModel.MinimumChartHeight));
			Assert.That(dailyViewsClonesMaxValue_Final, Is.GreaterThanOrEqualTo(dailyViewsClonesMaxValue_Initial));

			Assert.That(maxDailyStarsValue_Initial, Is.EqualTo(TrendsViewModel.MinimumChartHeight));
			Assert.That(maxDailyStarsValue_Final, Is.GreaterThanOrEqualTo(maxDailyStarsValue_Initial));

			Assert.That(dailyViewsClonesMinValue_Initial, Is.EqualTo(0));
			Assert.That(dailyViewsClonesMinValue_Initial, Is.EqualTo(dailyViewsClonesMinValue_Final));

			Assert.That(minDailyStarsValue_Initial, Is.EqualTo(0));
			Assert.That(minDailyStarsValue_Initial, Is.EqualTo(minDailyStarsValue_Final));

			Assert.That(minViewsClonesDate_Initial.Date, Is.EqualTo(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13)).Date));
			Assert.That(minViewsClonesDate_Final.Date, Is.LessThanOrEqualTo(minViewsClonesDate_Initial.Date));

			Assert.That(maxViewsClonesDate_Initial.Date, Is.EqualTo(DateTimeOffset.UtcNow.Date));
			Assert.That(maxViewsClonesDate_Final.Date, Is.LessThanOrEqualTo(maxViewsClonesDate_Initial.Date));

			Assert.That(minDailyStarsDate_Final.Date, Is.LessThanOrEqualTo(minDailyStarsDate_Initial.Date));
			Assert.That(maxDailyStarsDate_Final.Date, Is.LessThanOrEqualTo(maxDailyStarsDate_Initial.Date));

			Assert.That(viewsClonesEmptyDataViewTitleText_Initial, Is.EqualTo(EmptyDataViewService.GetViewsClonesTitleText(RefreshState.Uninitialized)));
			Assert.That(viewsClonesEmptyDataViewTitleText_Final, Is.EqualTo(EmptyDataViewService.GetViewsClonesTitleText(RefreshState.Succeeded)));

			Assert.That(starsEmptyDataViewTitleText_Initial, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Uninitialized, trendsViewModel.TotalStars)));
			Assert.That(starsEmptyDataViewTitleText_Final, Is.EqualTo(EmptyDataViewService.GetStarsEmptyDataViewTitleText(RefreshState.Succeeded, trendsViewModel.TotalStars)));

			Assert.That(starsHeaderMessageText_Initial, Is.EqualTo(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Uninitialized, default)));
			Assert.That(starsHeaderMessageText_Final, Is.EqualTo(EmptyDataViewService.GetStarsHeaderMessageText(RefreshState.Succeeded, trendsViewModel.TotalStars)));
		});
	}

	[TestCase(true)]
	[TestCase(false)]
	[CancelAfter(_timeoutInMilliseconds)]
	public async Task FetchDataCommandTest_DemoUser(bool shouldCreateViewsClonesData)
	{
		//Arrange
		IReadOnlyList<DailyStarsModel> dailyStarsList_Initial, dailyStarsList_Final;
		IReadOnlyList<DailyViewsModel> dailyViewsList_Initial, dailyViewsList_Final;
		IReadOnlyList<DailyClonesModel> dailyClonesList_Initial, dailyClonesList_Final;

		Repository repository = CreateRepository(shouldCreateViewsClonesData);

		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		dailyStarsList_Initial = trendsViewModel.DailyStarsList;
		dailyViewsList_Initial = trendsViewModel.DailyViewsList;
		dailyClonesList_Initial = trendsViewModel.DailyClonesList;

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

		dailyStarsList_Final = trendsViewModel.DailyStarsList;
		dailyViewsList_Final = trendsViewModel.DailyViewsList;
		dailyClonesList_Final = trendsViewModel.DailyClonesList;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(dailyStarsList_Initial, Is.Empty);
			Assert.That(dailyViewsList_Initial, Is.Empty);
			Assert.That(dailyClonesList_Initial, Is.Empty);

			Assert.That(dailyStarsList_Final, Is.Not.Empty);
			Assert.That(dailyViewsList_Final, Is.Not.Empty);
			Assert.That(dailyClonesList_Final, Is.Not.Empty);
		});
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task UniqueClonesCardTappedCommandTest()
	{
		//Arrange
		bool isUniqueClonesSeriesVisible_Initial, isUniqueClonesSeriesVisible_AfterFetchDataCommand, isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand;
		string uniqueClonesStatisticsText_Initial, uniqueClonesStatisticsText_AfterFetchDataCommand, uniqueClonesStatisticsText_AfterViewsCardTappedCommand;

		Repository repository = CreateRepository();

		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		isUniqueClonesSeriesVisible_Initial = trendsViewModel.IsUniqueClonesSeriesVisible;
		uniqueClonesStatisticsText_Initial = trendsViewModel.UniqueClonesStatisticsText;

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

		isUniqueClonesSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsUniqueClonesSeriesVisible;
		uniqueClonesStatisticsText_AfterFetchDataCommand = trendsViewModel.UniqueClonesStatisticsText;

		trendsViewModel.UniqueClonesCardTappedCommand.Execute(null);

		isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsUniqueClonesSeriesVisible;
		uniqueClonesStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.UniqueClonesStatisticsText;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(uniqueClonesStatisticsText_Initial, Is.EqualTo(string.Empty));
			Assert.That(uniqueClonesStatisticsText_AfterFetchDataCommand, Is.EqualTo(repository.TotalUniqueClones.ToAbbreviatedText()));
			Assert.That(uniqueClonesStatisticsText_AfterViewsCardTappedCommand, Is.EqualTo(repository.TotalUniqueClones.ToAbbreviatedText()));

			Assert.That(isUniqueClonesSeriesVisible_Initial);
			Assert.That(isUniqueClonesSeriesVisible_AfterFetchDataCommand);
			Assert.That(isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand, Is.False);
		});
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task ClonesCardTappedCommandTest()
	{
		//Arrange
		bool isClonesSeriesVisible_Initial, isClonesSeriesVisible_AfterFetchDataCommand, isClonesSeriesVisible_AfterViewsCardTappedCommand;
		string clonesStatisticsText_Initial, clonesStatisticsText_AfterFetchDataCommand, clonesStatisticsText_AfterViewsCardTappedCommand;

		Repository repository = CreateRepository();

		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		isClonesSeriesVisible_Initial = trendsViewModel.IsClonesSeriesVisible;
		clonesStatisticsText_Initial = trendsViewModel.ClonesStatisticsText;

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

		isClonesSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsClonesSeriesVisible;
		clonesStatisticsText_AfterFetchDataCommand = trendsViewModel.ClonesStatisticsText;

		trendsViewModel.ClonesCardTappedCommand.Execute(null);

		isClonesSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsClonesSeriesVisible;
		clonesStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.ClonesStatisticsText;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(clonesStatisticsText_Initial, Is.EqualTo(string.Empty));
			Assert.That(clonesStatisticsText_AfterFetchDataCommand, Is.EqualTo(repository.TotalClones.ToAbbreviatedText()));
			Assert.That(clonesStatisticsText_AfterViewsCardTappedCommand, Is.EqualTo(repository.TotalClones.ToAbbreviatedText()));

			Assert.That(isClonesSeriesVisible_Initial);
			Assert.That(isClonesSeriesVisible_AfterFetchDataCommand);
			Assert.That(isClonesSeriesVisible_AfterViewsCardTappedCommand, Is.False);
		});
	}


	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task UniqueViewsCardTappedCommandTest()
	{
		//Arrange
		bool isUniqueViewsSeriesVisible_Initial, isUniqueViewsSeriesVisible_AfterFetchDataCommand, isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand;
		string uniqueViewsStatisticsText_Initial, uniqueViewsStatisticsText_AfterFetchDataCommand, uniqueViewsStatisticsText_AfterViewsCardTappedCommand;

		Repository repository = CreateRepository();

		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		isUniqueViewsSeriesVisible_Initial = trendsViewModel.IsUniqueViewsSeriesVisible;
		uniqueViewsStatisticsText_Initial = trendsViewModel.UniqueViewsStatisticsText;

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

		isUniqueViewsSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsUniqueViewsSeriesVisible;
		uniqueViewsStatisticsText_AfterFetchDataCommand = trendsViewModel.UniqueViewsStatisticsText;

		trendsViewModel.UniqueViewsCardTappedCommand.Execute(null);

		isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsUniqueViewsSeriesVisible;
		uniqueViewsStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.UniqueViewsStatisticsText;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(uniqueViewsStatisticsText_Initial, Is.EqualTo(string.Empty));
			Assert.That(uniqueViewsStatisticsText_AfterFetchDataCommand, Is.EqualTo(repository.TotalUniqueViews.ToAbbreviatedText()));
			Assert.That(uniqueViewsStatisticsText_AfterViewsCardTappedCommand, Is.EqualTo(repository.TotalUniqueViews.ToAbbreviatedText()));

			Assert.That(isUniqueViewsSeriesVisible_Initial);
			Assert.That(isUniqueViewsSeriesVisible_AfterFetchDataCommand);
			Assert.That(isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand, Is.False);
		});
	}

	[Test, CancelAfter(_timeoutInMilliseconds)]
	public async Task ViewsCardTappedCommandTest()
	{
		//Arrange
		bool isViewsSeriesVisible_Initial, isViewsSeriesVisible_AfterFetchDataCommand, isViewsSeriesVisible_AfterViewsCardTappedCommand;
		string viewsStatisticsText_Initial, viewsStatisticsText_AfterFetchDataCommand, viewsStatisticsText_AfterViewsCardTappedCommand;

		Repository repository = CreateRepository();

		var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		isViewsSeriesVisible_Initial = trendsViewModel.IsViewsSeriesVisible;
		viewsStatisticsText_Initial = trendsViewModel.ViewsStatisticsText;

		await trendsViewModel.FetchData(repository, CancellationToken.None).ConfigureAwait(false);

		isViewsSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsViewsSeriesVisible;
		viewsStatisticsText_AfterFetchDataCommand = trendsViewModel.ViewsStatisticsText;

		trendsViewModel.ViewsCardTappedCommand.Execute(null);

		isViewsSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsViewsSeriesVisible;
		viewsStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.ViewsStatisticsText;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(viewsStatisticsText_Initial, Is.EqualTo(string.Empty));
			Assert.That(viewsStatisticsText_AfterFetchDataCommand, Is.EqualTo(repository.TotalViews.ToAbbreviatedText()));
			Assert.That(viewsStatisticsText_AfterViewsCardTappedCommand, Is.EqualTo(repository.TotalViews.ToAbbreviatedText()));

			Assert.That(isViewsSeriesVisible_Initial);
			Assert.That(isViewsSeriesVisible_AfterFetchDataCommand);
			Assert.That(isViewsSeriesVisible_AfterViewsCardTappedCommand, Is.False);
		});
	}
}