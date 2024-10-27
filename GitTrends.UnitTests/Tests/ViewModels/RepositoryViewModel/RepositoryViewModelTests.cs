using System.ComponentModel;
using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

[NonParallelizable]
class RepositoryViewModelTests : BaseTest
{
	[Test, CancelAfter(600000)]
	public async Task PullToRefreshCommandTest_Authenticated()
	{
		//Arrange
		DateTimeOffset beforePullToRefresh, afterPullToRefresh;
		IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;

		string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
		string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		var repositoriesUpdatedInBackground = new List<Repository>();
		var fetchStarsInBackgroundTCS = new TaskCompletionSource<IReadOnlyList<Repository>>();
		RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase += HandleScheduleRetryRepositoriesStarsCompleted;

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		beforePullToRefresh = DateTimeOffset.UtcNow;
		emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Initial = repositoryViewModel.VisibleRepositoryList;
		emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		if (backgroundFetchService.QueuedForegroundJobsList.Any())
		{
			var fetchStarsInBackgroundTCSResult = await fetchStarsInBackgroundTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
			Assert.That(fetchStarsInBackgroundTCSResult, Is.Not.Empty);
		}
		else
		{
			RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase -= HandleScheduleRetryRepositoriesStarsCompleted;
		}

		afterPullToRefresh = DateTimeOffset.UtcNow;
		emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Final = repositoryViewModel.VisibleRepositoryList;
		emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(visibleRepositoryList_Initial, Is.Empty);
			Assert.That(visibleRepositoryList_Final, Is.Not.Empty);

			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Succeeded, false)));

			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Succeeded, false)));

			Assert.That(visibleRepositoryList_Final.Any(static x => x is { OwnerLogin: GitHubConstants.GitTrendsRepoOwner, Name: GitHubConstants.GitTrendsRepoName }), Is.True);


			foreach (var repository in repositoriesUpdatedInBackground)
				Assert.That(visibleRepositoryList_Final.Any(x => x.Url == repository.Url));

			foreach (var repository in visibleRepositoryList_Final)
			{
				Assert.That(repository.DailyClonesList, Is.Not.Null);
				Assert.That(repository.DailyViewsList, Is.Not.Null);
				Assert.That(repository.StarredAt, Is.Not.Null);
				Assert.That(repository.TotalClones, Is.Not.Null);
				Assert.That(repository.TotalUniqueClones, Is.Not.Null);
				Assert.That(repository.TotalUniqueViews, Is.Not.Null);
				Assert.That(repository.TotalViews, Is.Not.Null);

				Assert.That(repository.IsArchived, Is.False);
				Assert.That(repository.IsFork, Is.False);

				Assert.That(beforePullToRefresh, Is.LessThan(repository.DataDownloadedAt));
				Assert.That(afterPullToRefresh, Is.GreaterThan(repository.DataDownloadedAt));
			}
		});

		void HandleScheduleRetryRepositoriesStarsCompleted(object? sender, Repository e)
		{
			if (backgroundFetchService.QueuedForegroundJobsList.Count <= 1) // Job is removed from QueuedForegroundJobsList after `RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase` event fires
			{
				RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase -= HandleScheduleRetryRepositoriesStarsCompleted;
			}

			Assert.Multiple(() =>
			{
				Assert.That(e.DailyClonesList, Is.Not.Null);
				Assert.That(e.DailyViewsList, Is.Not.Null);
				Assert.That(e.StarredAt, Is.Not.Null);
				Assert.That(e.TotalClones, Is.Not.Null);
				Assert.That(e.TotalUniqueClones, Is.Not.Null);
				Assert.That(e.TotalUniqueViews, Is.Not.Null);
				Assert.That(e.TotalViews, Is.Not.Null);
			});

			repositoriesUpdatedInBackground.Add(e);

			if (backgroundFetchService.QueuedForegroundJobsList.Count <= 1)
			{
				fetchStarsInBackgroundTCS.SetResult(repositoriesUpdatedInBackground);
			}
		}
	}


	[Test, CancelAfter(600000)]
	public async Task PullToRefreshCommandTest_ShouldIncludeOrganizationsChanged()
	{
		//Arrange
		var handlePullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

		RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		var pullToRefreshCommandTask = repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token);
		gitHubUserService.ShouldIncludeOrganizations = !gitHubUserService.ShouldIncludeOrganizations;

		await pullToRefreshCommandTask.ConfigureAwait(false);
		var handlePullToRefreshFailedResult = await handlePullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repositoryViewModel.VisibleRepositoryList, Is.Empty);
			Assert.That(handlePullToRefreshFailedResult, Is.InstanceOf<ErrorPullToRefreshEventArgs>());
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			handlePullToRefreshFailedTCS.SetResult(e);
		}
	}

	[Test, CancelAfter(600000)]
	public async Task PullToRefreshCommandTest_LoggedOut()
	{
		//Arrange
		var handlePullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		var pullToRefreshCommandTask = repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token);
		await gitHubAuthenticationService.LogOut(TestCancellationTokenSource.Token).ConfigureAwait(false);

		await pullToRefreshCommandTask.ConfigureAwait(false);
		var handlePullToRefreshFailedResult = await handlePullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repositoryViewModel.VisibleRepositoryList, Is.Empty);
			Assert.That(handlePullToRefreshFailedResult, Is.InstanceOf<PullToRefreshFailedEventArgs>());
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			handlePullToRefreshFailedTCS.SetResult(e);
		}
	}

	[Test, CancelAfter(600000)]
	public async Task PullToRefreshCommandTest_AuthorizeSessionStarted()
	{
		//Arrange
		var handlePullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		//Act
		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		var pullToRefreshCommandTask = repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token);
		try
		{
			await gitHubAuthenticationService.AuthorizeSession(new Uri("https://gittrends"), CancellationToken.None).ConfigureAwait(false);
		}
		catch
		{

		}

		await pullToRefreshCommandTask.ConfigureAwait(false);
		var handlePullToRefreshFailedResult = await handlePullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repositoryViewModel.VisibleRepositoryList, Is.Empty);
			Assert.That(handlePullToRefreshFailedResult, Is.InstanceOf<ErrorPullToRefreshEventArgs>());
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;
			handlePullToRefreshFailedTCS.SetResult(e);
		}
	}

	[Test, CancelAfter(600000)]
	public async Task PullToRefreshCommandTest_Unauthenticated()
	{
		//Arrange
		IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;
		string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
		string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();

		bool didPullToRefreshFailedFire = false;
		var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

		RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

		//Act
		emptyDataViewTitle_Initial = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Initial = repositoryViewModel.VisibleRepositoryList;
		emptyDataViewDescription_Initial = repositoryViewModel.EmptyDataViewDescription;

		var pullToRefreshCommandTask = repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token);

		await pullToRefreshCommandTask.ConfigureAwait(false);
		var pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		emptyDataViewTitle_Final = repositoryViewModel.EmptyDataViewTitle;
		visibleRepositoryList_Final = repositoryViewModel.VisibleRepositoryList;
		emptyDataViewDescription_Final = repositoryViewModel.EmptyDataViewDescription;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didPullToRefreshFailedFire);
			Assert.That(pullToRefreshFailedEventArgs, Is.InstanceOf(typeof(LoginExpiredPullToRefreshEventArgs)));

			Assert.That(visibleRepositoryList_Initial, Is.Empty);
			Assert.That(visibleRepositoryList_Final, Is.Empty);

			Assert.That(emptyDataViewTitle_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewTitle_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryTitleText(RefreshState.LoginExpired, true)));

			Assert.That(emptyDataViewDescription_Initial, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true)));
			Assert.That(emptyDataViewDescription_Final, Is.EqualTo(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.LoginExpired, true)));
		});

		void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
		{
			RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;

			didPullToRefreshFailedFire = true;
			pullToRefreshFailedTCS.SetResult(e);
		}
	}

	[Test]
	public async Task FilterRepositoriesCommandTest_InvalidRepositoryName()
	{
		//Arrange
		Repository repository;
		int repositoryListCount_Initial, repositoryListCount_Final;
		IReadOnlyList<Repository> repositoryList_Initial, repositoryList_Final;

		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		repositoryList_Initial = repositoryViewModel.VisibleRepositoryList;
		repository = repositoryList_Initial.First();
		repositoryListCount_Initial = repositoryList_Initial.Count;

		repositoryViewModel.SetSearchBarTextCommand.Execute(repository.Name + DemoDataConstants.GetRandomText());

		repositoryList_Final = repositoryViewModel.VisibleRepositoryList;
		repositoryListCount_Final = repositoryList_Final.Count;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repositoryViewModel.VisibleRepositoryList, Is.Empty);
			Assert.That(repositoryListCount_Initial, Is.GreaterThan(0));
			Assert.That(repositoryListCount_Final, Is.LessThan(repositoryListCount_Initial));
		});
	}

	[Test]
	public async Task FilterRepositoriesCommandTest_ValidRepositoryName()
	{
		//Arrange
		Repository repository;
		int repositoryListCount_Initial, repositoryListCount_Final;
		IReadOnlyList<Repository> repositoryList_Initial, repositoryList_Final;

		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		repositoryList_Initial = repositoryViewModel.VisibleRepositoryList;
		repository = repositoryList_Initial.First();
		repositoryListCount_Initial = repositoryList_Initial.Count;

		repositoryViewModel.SetSearchBarTextCommand.Execute(repository.Name);

		repositoryList_Final = repositoryViewModel.VisibleRepositoryList;
		repositoryListCount_Final = repositoryList_Final.Count;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repositoryViewModel.VisibleRepositoryList.ToArray(), Does.Contain(repository));
			Assert.That(repositoryListCount_Initial, Is.GreaterThan(1));
			Assert.That(repositoryListCount_Final, Is.LessThan(repositoryListCount_Initial));
		});
	}

	[TestCase((SortingOption)int.MinValue)]
	[TestCase((SortingOption)int.MaxValue)]
	[TestCase((SortingOption)100)]
	[TestCase((SortingOption)(-1))]
	public async Task SortRepositoriesCommandTest_InvalidSortingOption(SortingOption sortingOption)
	{
		//Arrange
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.Throws<InvalidEnumArgumentException>(() => repositoryViewModel.SortRepositoriesCommand.Execute(sortingOption));
	}

	[TestCase(SortingOption.Clones)]
	[TestCase(SortingOption.Forks)]
	[TestCase(SortingOption.Issues)]
	[TestCase(SortingOption.Stars)]
	[TestCase(SortingOption.UniqueClones)]
	[TestCase(SortingOption.UniqueViews)]
	[TestCase(SortingOption.Views)]
	public async Task SortRepositoriesCommandTest_ValidSortingOption(SortingOption sortingOption)
	{
		//Arrange
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		//Act
		await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		repositoryViewModel.SortRepositoriesCommand.Execute(sortingOption);

		//Assert
		AssertRepositoriesSorted(repositoryViewModel.VisibleRepositoryList, sortingOption);

		//Act
		repositoryViewModel.SortRepositoriesCommand.Execute(sortingOption);

		//Assert
		AssertRepositoriesReversedSorted(repositoryViewModel.VisibleRepositoryList, sortingOption);
	}

	[TestCase(true)]
	[TestCase(false)]
	public async Task ToggleIsFavoriteCommandTest(bool isDemo)
	{
		//Arrange
		IReadOnlyList<string> favoriteUrls;
		Repository repository_initial, repository_favorite, repository_final;

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();

		//Act
		if (isDemo)
			await gitHubAuthenticationService.ActivateDemoUser(TestCancellationTokenSource.Token).ConfigureAwait(false);
		else
			await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await repositoryViewModel.ExecuteRefreshCommand.ExecuteAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		repository_initial = repositoryViewModel.VisibleRepositoryList.First();


		await repositoryViewModel.ToggleIsFavoriteCommand.ExecuteAsync(repository_initial).ConfigureAwait(false);
		repository_favorite = repositoryViewModel.VisibleRepositoryList.First();
		favoriteUrls = await repositoryDatabase.GetFavoritesUrls(TestCancellationTokenSource.Token).ConfigureAwait(false);

		await repositoryViewModel.ToggleIsFavoriteCommand.ExecuteAsync(repository_favorite).ConfigureAwait(false);
		repository_final = repositoryViewModel.VisibleRepositoryList.First();

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(repository_initial.IsFavorite, Is.Null);
			Assert.That(repository_favorite.IsFavorite, Is.True);
			Assert.That(repository_final.IsFavorite, Is.False);

			if (isDemo)
				Assert.That(favoriteUrls, Is.Empty);
			else
				Assert.That(repository_favorite.Url, Is.EqualTo(favoriteUrls.First()));
		});
	}

	static void AssertRepositoriesReversedSorted(in IEnumerable<Repository> repositories, in SortingOption sortingOption)
	{
		var topRepository = repositories.First();
		var secondTopRepository = repositories.Skip(1).First();

		var lastRepository = repositories.Last();

		switch (sortingOption)
		{
			case SortingOption.Views when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalViews);
				Assert.That(topRepository.TotalViews, Is.GreaterThanOrEqualTo(secondTopRepository.TotalViews));
				break;
			case SortingOption.Views:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalViews);
				Assert.That(secondTopRepository.TotalViews, Is.GreaterThanOrEqualTo(lastRepository.TotalViews));
				break;
			case SortingOption.Stars when topRepository.IsTrending == secondTopRepository.IsTrending:
				Assert.That(topRepository.StarCount, Is.LessThanOrEqualTo(secondTopRepository.StarCount));
				break;
			case SortingOption.Stars:
				Assert.That(secondTopRepository.StarCount, Is.LessThanOrEqualTo(lastRepository.StarCount));
				break;
			case SortingOption.Forks when topRepository.IsTrending == secondTopRepository.IsTrending:
				Assert.That(topRepository.ForkCount, Is.LessThanOrEqualTo(secondTopRepository.ForkCount));
				break;
			case SortingOption.Forks:
				Assert.That(secondTopRepository.ForkCount, Is.LessThanOrEqualTo(lastRepository.ForkCount));
				break;
			case SortingOption.Issues when topRepository.IsTrending == secondTopRepository.IsTrending:
				Assert.That(topRepository.IssuesCount, Is.LessThanOrEqualTo(secondTopRepository.IssuesCount));
				break;
			case SortingOption.Issues:
				Assert.That(secondTopRepository.IssuesCount, Is.LessThanOrEqualTo(lastRepository.IssuesCount));
				break;
			case SortingOption.Clones when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalClones);
				Assert.That(topRepository.TotalClones, Is.LessThanOrEqualTo(secondTopRepository.TotalClones));
				break;
			case SortingOption.Clones:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalClones);
				Assert.That(secondTopRepository.TotalClones, Is.LessThanOrEqualTo(lastRepository.TotalClones));
				break;
			case SortingOption.UniqueClones when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalUniqueClones);
				Assert.That(topRepository.TotalUniqueClones, Is.LessThanOrEqualTo(secondTopRepository.TotalUniqueClones));
				break;
			case SortingOption.UniqueClones:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalUniqueClones);
				Assert.That(secondTopRepository.TotalUniqueClones, Is.LessThanOrEqualTo(lastRepository.TotalUniqueClones));
				break;
			case SortingOption.UniqueViews when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalUniqueViews);
				Assert.That(topRepository.TotalUniqueViews, Is.LessThanOrEqualTo(secondTopRepository.TotalUniqueViews));
				break;
			case SortingOption.UniqueViews:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalUniqueViews);
				Assert.That(secondTopRepository.TotalUniqueViews, Is.LessThanOrEqualTo(lastRepository.TotalUniqueViews));
				break;
			default:
				throw new NotSupportedException();
		}
	}

	static void AssertRepositoriesSorted(in IEnumerable<Repository> repositories, in SortingOption sortingOption)
	{
		var topRepository = repositories.First();
		var secondTopRepository = repositories.Skip(1).First();

		var lastRepository = repositories.Last();

		switch (sortingOption)
		{
			case SortingOption.Views when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalViews);
				Assert.That(topRepository.TotalViews, Is.LessThanOrEqualTo(secondTopRepository.TotalViews));
				break;
			case SortingOption.Views:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalViews);
				Assert.That(secondTopRepository.TotalViews, Is.LessThanOrEqualTo(lastRepository.TotalViews));
				break;
			case SortingOption.Stars when topRepository.IsTrending == secondTopRepository.IsTrending:
				Assert.That(topRepository.StarCount, Is.GreaterThanOrEqualTo(secondTopRepository.StarCount));
				break;
			case SortingOption.Stars:
				Assert.That(secondTopRepository.StarCount, Is.GreaterThanOrEqualTo(lastRepository.StarCount));
				break;
			case SortingOption.Forks when topRepository.IsTrending == secondTopRepository.IsTrending:
				Assert.That(topRepository.ForkCount, Is.GreaterThanOrEqualTo(secondTopRepository.ForkCount));
				break;
			case SortingOption.Forks:
				Assert.That(secondTopRepository.ForkCount, Is.GreaterThanOrEqualTo(lastRepository.ForkCount));
				break;
			case SortingOption.Issues when topRepository.IsTrending == secondTopRepository.IsTrending:
				Assert.That(topRepository.IssuesCount, Is.GreaterThanOrEqualTo(secondTopRepository.IssuesCount));
				break;
			case SortingOption.Issues:
				Assert.That(secondTopRepository.IssuesCount, Is.GreaterThanOrEqualTo(lastRepository.IssuesCount));
				break;
			case SortingOption.Clones when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalClones);
				Assert.That(topRepository.TotalClones, Is.GreaterThanOrEqualTo(secondTopRepository.TotalClones));
				break;
			case SortingOption.Clones:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalClones);
				Assert.That(secondTopRepository.TotalClones, Is.GreaterThanOrEqualTo(lastRepository.TotalClones));
				break;
			case SortingOption.UniqueClones when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalUniqueClones);
				Assert.That(topRepository.TotalUniqueClones, Is.GreaterThanOrEqualTo(secondTopRepository.TotalUniqueClones));
				break;
			case SortingOption.UniqueClones:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalUniqueClones);
				Assert.That(secondTopRepository.TotalUniqueClones, Is.GreaterThanOrEqualTo(lastRepository.TotalUniqueClones));
				break;
			case SortingOption.UniqueViews when topRepository.IsTrending == secondTopRepository.IsTrending:
				ArgumentNullException.ThrowIfNull(secondTopRepository.TotalUniqueViews);
				Assert.That(topRepository.TotalUniqueViews, Is.GreaterThanOrEqualTo(secondTopRepository.TotalUniqueViews));
				break;
			case SortingOption.UniqueViews:
				ArgumentNullException.ThrowIfNull(lastRepository.TotalUniqueViews);
				Assert.That(secondTopRepository.TotalUniqueViews, Is.GreaterThanOrEqualTo(lastRepository.TotalUniqueViews));
				break;
			default:
				throw new NotSupportedException();
		}
	}
}