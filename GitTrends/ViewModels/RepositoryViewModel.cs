using System.Net;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubApiStatus;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Refit;

namespace GitTrends;

public partial class RepositoryViewModel : BaseViewModel
{
	static readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new();

	readonly GitHubUserService _gitHubUserService;
	readonly RepositoryDatabase _repositoryDatabase;
	readonly DeepLinkingService _deepLinkingService;
	readonly GitHubApiV3Service _gitHubApiV3Service;
	readonly MobileSortingService _mobileSortingService;
	readonly IGitHubApiStatusService _gitHubApiStatusService;
	readonly BackgroundFetchService _backgroundFetchService;
	readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
	readonly GitHubAuthenticationService _gitHubAuthenticationService;
	readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService;

	IReadOnlyList<Repository> _repositoryList = [];
	RefreshState _refreshState;

    public RepositoryViewModel(
		IDispatcher dispatcher,
		IAnalyticsService analyticsService,
		GitHubUserService gitHubUserService,
		RepositoryDatabase repositoryDatabase,
		DeepLinkingService deepLinkingService,
		GitHubApiV3Service gitHubApiV3Service,
		MobileSortingService mobileSortingService,
		IGitHubApiStatusService gitHubApiStatusService,
		BackgroundFetchService backgroundFetchService,
		GitHubGraphQLApiService gitHubGraphQLApiService,
		GitHubAuthenticationService gitHubAuthenticationService,
		GitHubApiRepositoriesService gitHubApiRepositoriesService) : base(analyticsService, dispatcher)
	{
		_gitHubUserService = gitHubUserService;
		_repositoryDatabase = repositoryDatabase;
		_deepLinkingService = deepLinkingService;
		_gitHubApiV3Service = gitHubApiV3Service;
		_mobileSortingService = mobileSortingService;
		_gitHubApiStatusService = gitHubApiStatusService;
		_backgroundFetchService = backgroundFetchService;
		_gitHubGraphQLApiService = gitHubGraphQLApiService;
		_gitHubAuthenticationService = gitHubAuthenticationService;
		_gitHubApiRepositoriesService = gitHubApiRepositoriesService;

		RefreshState = RefreshState.Uninitialized;

		NotificationService.SortingOptionRequested += HandleSortingOptionRequested;

		LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

		GitHubApiRepositoriesService.RepositoryUriNotFound += HandleRepositoryUriNotFound;

		GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
		GitHubAuthenticationService.LoggedOut += HandleGitHubAuthenticationServiceLoggedOut;
		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

		TrendsViewModel.RepositorySavedToDatabase += HandleTrendsViewModelRepositorySavedToDatabase;

		GitHubUserService.ShouldIncludeOrganizationsChanged += HandleShouldIncludeOrganizationsChanged;

		RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase += HandleScheduleRetryUpdatedRepositoriesStarsCompleted;
		RetryRepositoriesViewsClonesStarsJob.JobCompleted += HandleJobCompleted;

		RepositoryPage.SearchBarTextChanged += HandleSearchBarTextChanged;

		UpdateText();
	}

	public static event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
	{
		add => _pullToRefreshFailedEventManager.AddEventHandler(value);
		remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
	}
	
	[ObservableProperty]
	public partial bool IsRefreshing { get; internal set; }
	
	[ObservableProperty]
	public partial string SearchBarText { get; private set; } = string.Empty;

	[ObservableProperty]
	public partial string TitleText { get; private set; } = string.Empty;

	[ObservableProperty]
	public partial string TotalButtonText { get; private set; } = string.Empty;

	[ObservableProperty]
	public partial string EmptyDataViewTitle { get; private set; } = string.Empty;

	[ObservableProperty]
	public partial string EmptyDataViewDescription { get; private set; } = string.Empty;

	[ObservableProperty]
	public partial IReadOnlyList<Repository> VisibleRepositoryList { get; private set; } = [];

	RefreshState RefreshState
	{
		get => _refreshState;
		set
		{
			EmptyDataViewTitle = EmptyDataViewService.GetRepositoryTitleText(value, !_repositoryList.Any());
			EmptyDataViewDescription = EmptyDataViewService.GetRepositoryDescriptionText(value, !_repositoryList.Any());
			_refreshState = value;
		}
	}

	static IEnumerable<Repository> GetRepositoriesFilteredBySearchBar(in IEnumerable<Repository> repositories, string searchBarText)
	{
		return string.IsNullOrWhiteSpace(searchBarText) ? repositories : repositories.Where(x => x.Name.Contains(searchBarText, StringComparison.OrdinalIgnoreCase));
	}

	static bool IsNavigateToRepositoryWebsiteCommandEnabled(Repository? repository) => repository?.IsRepositoryUrlValid() is true;

	[RelayCommand(AllowConcurrentExecutions = true)]
	async Task ExecuteRefresh(CancellationToken token)
	{
		const int minimumBatchCount = 100;

		HttpResponseMessage? finalResponse = null;
		IReadOnlyList<Repository>? repositoriesFromDatabase = null;
		var saveCompletedRepositoryToDatabaseTaskList = new List<Task>();

		var cancellationTokenSource = new CancellationTokenSource();
		GitHubAuthenticationService.LoggedOut += HandleLoggedOut;
		GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;
		GitHubUserService.ShouldIncludeOrganizationsChanged += HandleShouldIncludeOrganizationsChanged;

		AnalyticsService.Track("Refresh Triggered", "Sorting Option", _mobileSortingService.CurrentOption.ToString());

		var repositoriesFromDatabaseTask = _repositoryDatabase.GetRepositories(token);

		try
		{
			#region Get Visible RepositoryList Data in Foreground

			var favoriteRepositoryUrls = await _repositoryDatabase.GetFavoritesUrls(token).ConfigureAwait(false);

			var repositoryList = new List<Repository>();
			await foreach (var repository in _gitHubGraphQLApiService.GetRepositories(_gitHubUserService.Alias, cancellationTokenSource.Token).ConfigureAwait(false))
			{
				if (favoriteRepositoryUrls.Contains(repository.Url))
					repositoryList.Add(repository with
					{
						IsFavorite = true
					});
				else
					repositoryList.Add(repository);

				//Batch the VisibleRepositoryList Updates to avoid overworking the UI Thread
				if (!_gitHubUserService.IsDemoUser && repositoryList.Count > minimumBatchCount)
				{
					//Only display the first update to avoid unnecessary work on the UIThread
					var shouldUpdateVisibleRepositoryList = !VisibleRepositoryList.Any() || repositoryList.Count >= minimumBatchCount;
					AddRepositoriesToCollection(repositoryList, SearchBarText, shouldUpdateVisibleRepositoryList);
					repositoryList.Clear();
				}
			}

			//Add Remaining Repositories to _repositoryList
			AddRepositoriesToCollection(repositoryList, SearchBarText);

			repositoriesFromDatabase = await repositoriesFromDatabaseTask.ConfigureAwait(false);

			var repositoriesFromDatabaseThatDontRequireUpdating = repositoriesFromDatabase.Where(x => x.ContainsViewsClonesData // Ensure the repository contains data for Views + Clones
				&& x.DataDownloadedAt >= DateTimeOffset.Now.Subtract(CachedDataConstants.ViewsClonesCacheLifeSpan) // Cached repositories that have been updated in the past 12 hours				
				&& _repositoryList.Any(y => y.Url == x.Url)); // Ensure was retrieved from GitHub)

			AddRepositoriesToCollection(repositoriesFromDatabaseThatDontRequireUpdating, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesData);

			var viewsClonesRepositoriesList = new List<Repository>();
			await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(_repositoryList.Where(static x => !x.ContainsViewsClonesData), cancellationTokenSource.Token).ConfigureAwait(false))
			{
				viewsClonesRepositoriesList.Add(retrievedRepositoryWithViewsAndClonesData);

				if (!_gitHubUserService.IsDemoUser)
				{
					//Batch the VisibleRepositoryList Updates to avoid overworking the UI Thread
					if (viewsClonesRepositoriesList.Count > minimumBatchCount)
					{
						AddRepositoriesToCollection(viewsClonesRepositoriesList, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesData);
						viewsClonesRepositoriesList.Clear();
					}
				}
			}

			//Add Remaining Repositories to VisibleRepositoryList
			AddRepositoriesToCollection(viewsClonesRepositoriesList, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesData);

			#endregion

			IsRefreshing = false;

			#region Get StarGazer Data in Background

			// The StarGazer data can be gathered in the background because the data only appears if the user navigates to the StarsTrendsView
			// The data is gathered in the background to optimize the Pull-To-Refresh time visible to the user
			repositoriesFromDatabaseThatDontRequireUpdating = repositoriesFromDatabase.Where(x => x.ContainsViewsClonesStarsData // Ensure the repository contains data for Views + Clones + Stars
				&& x.DataDownloadedAt >= DateTimeOffset.Now.Subtract(CachedDataConstants.StarsDataCacheLifeSpan) // Cached repositories that have been updated in the past 12 hours				
				&& _repositoryList.Any(y => y.Url == x.Url)); // Ensure was retrieved from GitHub)

			AddRepositoriesToCollection(repositoriesFromDatabaseThatDontRequireUpdating, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesStarsData);


			var repositoriesWithoutStarsDataAndOver1000Stars = _repositoryList.Where(static x => x is { ContainsStarsData: false, StarCount: > 1000 });
			var repositoriesWithoutStarsDataAndLessThan1000Stars = _repositoryList.Where(static x => x is { ContainsStarsData: false, StarCount: <= 1000 });

			// Fetch Stars Data in Background for Repositories Containing Over 1000 Stars
			// GitHub API limits us to 100 StarGazers per Request, meaning that a repository with 24K Stars requires 240 round-trips from GitTrends to GitHub's servers to aggregate the data
			// This data is not displayed in the Repository Page
			foreach (var repository in repositoriesWithoutStarsDataAndOver1000Stars)
				_backgroundFetchService.TryScheduleRetryRepositoriesStars(repository);

			var starredRepositoriesList = new List<Repository>();
			await foreach (var retrievedRepositoryWithStarsData in _gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(repositoriesWithoutStarsDataAndLessThan1000Stars, cancellationTokenSource.Token).ConfigureAwait(false))
			{
				starredRepositoriesList.Add(retrievedRepositoryWithStarsData);

				if (!_gitHubUserService.IsDemoUser)
				{
					var saveRepositoryToDatabaseTask = _repositoryDatabase.SaveRepository(retrievedRepositoryWithStarsData, token);
					saveCompletedRepositoryToDatabaseTaskList.Add(saveRepositoryToDatabaseTask);

					//Batch the VisibleRepositoryList Updates to avoid overworking the UI Thread
					if (starredRepositoriesList.Count > minimumBatchCount)
					{
						AddRepositoriesToCollection(starredRepositoriesList, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesStarsData);
						starredRepositoriesList.Clear();
					}
				}
			}

			AddRepositoriesToCollection(starredRepositoriesList, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesStarsData);

			#endregion

			if (!_gitHubUserService.IsDemoUser)
			{
				//Rate Limiting may cause some data to not return successfully from the GitHub API
				var missingRepositories = _gitHubUserService.ShouldIncludeOrganizations switch
				{
					true => getDistinctRepositories(_repositoryList, repositoriesFromDatabase, x => x.ContainsViewsClonesStarsData),
					false => getDistinctRepositories(_repositoryList, repositoriesFromDatabase, x => x.ContainsViewsClonesStarsData && x.OwnerLogin == _gitHubUserService.Alias)
				};

				AddRepositoriesToCollection(missingRepositories, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesData);

				//Call EnsureSuccessStatusCode to confirm the above API calls executed successfully
				finalResponse = await _gitHubApiV3Service.GetGitHubApiResponse(cancellationTokenSource.Token).ConfigureAwait(false);
				finalResponse.EnsureSuccessStatusCode();
			}

			RefreshState = RefreshState.Succeeded;
		}
		catch (Exception e) when ((e is ApiException { StatusCode: HttpStatusCode.Unauthorized })
			|| (e is HttpRequestException && finalResponse?.StatusCode is HttpStatusCode.Unauthorized))
		{
			OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

			await _gitHubAuthenticationService.LogOut(token).ConfigureAwait(false);
			await _repositoryDatabase.DeleteAllData(token).ConfigureAwait(false);

			SetRepositoriesCollection([], SearchBarText);
		}
		catch (Exception e) when (_gitHubApiStatusService.IsAbuseRateLimit(e, out var retryTimeSpan)
			|| (e is HttpRequestException && finalResponse is not null && _gitHubApiStatusService.IsAbuseRateLimit(finalResponse.Headers, out retryTimeSpan)))
		{
			repositoriesFromDatabase ??= await repositoriesFromDatabaseTask.ConfigureAwait(false);

			//Rate Limiting may cause some data to not return successfully from the GitHub API
			var missingRepositories = _gitHubUserService.ShouldIncludeOrganizations switch
			{
				true => getDistinctRepositories(_repositoryList, repositoriesFromDatabase),
				false => getDistinctRepositories(_repositoryList, repositoriesFromDatabase, x => x.OwnerLogin == _gitHubUserService.Alias)
			};

			AddRepositoriesToCollection(missingRepositories, SearchBarText, duplicateRepositoryPriorityFilter: x => x.ContainsViewsClonesData);

			foreach (var repositoryToUpdate in _repositoryList.Where(static x => !x.ContainsViewsClonesStarsData // Ensure the repository contains data for Views + Clones
				&& x.DataDownloadedAt < DateTimeOffset.Now.Subtract(TimeSpan.FromHours(12)))) // Cached repositories that have been updated in the past 12 hours
			{
				_backgroundFetchService.TryScheduleRetryRepositoriesStars(repositoryToUpdate, retryTimeSpan.Value);
			}

			var abuseLimit = new AbuseLimitPullToRefreshEventArgs(retryTimeSpan.Value, VisibleRepositoryList.Any());
			OnPullToRefreshFailed(abuseLimit);
		}
		catch (Exception e) when (_gitHubApiStatusService.HasReachedMaximumApiCallLimit(e)
			|| (e is HttpRequestException && finalResponse is not null && finalResponse.Headers.DoesContainGitHubRateLimitHeader() && _gitHubApiStatusService.HasReachedMaximumApiCallLimit(finalResponse.Headers)))
		{
			var responseHeaders = e switch
			{
				ApiException exception => exception.Headers,
				GraphQLException graphQLException => graphQLException.ResponseHeaders,
				HttpRequestException when finalResponse is not null => finalResponse.Headers,
				_ => throw new NotSupportedException()
			};

			var maximumApiRequestsReachedEventArgs = new MaximumApiRequestsReachedEventArgs(_gitHubApiStatusService.GetRateLimitResetDateTime(responseHeaders));
			OnPullToRefreshFailed(maximumApiRequestsReachedEventArgs);

			SetRepositoriesCollection([], SearchBarText);
		}
		catch (Exception e)
		{
			AnalyticsService.Report(e);

			repositoriesFromDatabase ??= await repositoriesFromDatabaseTask.ConfigureAwait(false);

			SetRepositoriesCollection(repositoriesFromDatabase, SearchBarText);

			if (repositoriesFromDatabase.Any())
			{
				var dataDownloadedAt = repositoriesFromDatabase.Max(static x => x.DataDownloadedAt);
				OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs($"{RepositoryPageConstants.DisplayingDataFrom} {dataDownloadedAt.ToLocalTime():dd MMMM @ HH:mm}"));
			}
			else
			{
				OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs(RepositoryPageConstants.CheckInternetConnectionTryAgain));
			}
		}
		finally
		{
			GitHubAuthenticationService.LoggedOut -= HandleLoggedOut;
			GitHubAuthenticationService.AuthorizeSessionStarted -= HandleAuthorizeSessionStarted;
			GitHubUserService.ShouldIncludeOrganizationsChanged -= HandleShouldIncludeOrganizationsChanged;

			if (cancellationTokenSource.IsCancellationRequested)
				UpdateListForLoggedOutUser();

			IsRefreshing = false;

			await Task.WhenAll(saveCompletedRepositoryToDatabaseTaskList).ConfigureAwait(false);
		}

		static IReadOnlyList<Repository> getDistinctRepositories(in IEnumerable<Repository> repositoriesList1, in IEnumerable<Repository> repositoriesList2, Func<Repository, bool>? filter = null) =>
			[.. repositoriesList1.Concat(repositoriesList2).Where(filter ?? (_ => true)).GroupBy(static x => x.Url).Where(g => g.Count() is 1).Select(g => g.First())];

		void HandleLoggedOut(object? sender, EventArgs e) => cancellationTokenSource.Cancel();
		void HandleAuthorizeSessionStarted(object? sender, EventArgs e) => cancellationTokenSource.Cancel();
		void HandleShouldIncludeOrganizationsChanged(object? sender, bool e) => cancellationTokenSource.Cancel();
	}

	[RelayCommand]
	async Task ToggleIsFavorite(Repository repository, CancellationToken token)
	{
		await Task.Yield();

		var updatedRepository = repository with
		{
			IsFavorite = repository.IsFavorite.HasValue ? !repository.IsFavorite : true
		};

		AnalyticsService.Track("IsFavorite Toggled", nameof(Repository.IsFavorite), updatedRepository.IsFavorite.Value.ToString());

		List<Repository> updatedRepositoryList = [.. VisibleRepositoryList];
		updatedRepositoryList.Remove(repository);
		updatedRepositoryList.Add(updatedRepository);

		SetRepositoriesCollection(updatedRepositoryList, SearchBarText);

		if (!_gitHubUserService.IsDemoUser)
			await _repositoryDatabase.SaveRepository(updatedRepository, token).ConfigureAwait(false);
	}

	[RelayCommand(CanExecute = nameof(IsNavigateToRepositoryWebsiteCommandEnabled))]
	Task NavigateToRepositoryWebsite(Repository? repository, CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(repository);

		AnalyticsService.Track("Open External Repository Link Tapped", nameof(repository.Url), repository.Url);
		return _deepLinkingService.OpenApp(GitHubConstants.AppScheme, repository.Url, repository.Url, token);
	}

	[RelayCommand]
	void SortRepositories(SortingOption option)
	{
		if (_mobileSortingService.CurrentOption == option)
			_mobileSortingService.IsReversed = !_mobileSortingService.IsReversed;
		else
			_mobileSortingService.IsReversed = false;

		_mobileSortingService.CurrentOption = option;

		AnalyticsService.Track($"{nameof(SortingOption)} Changed", new Dictionary<string, string>
		{
			{
				nameof(MobileSortingService) + nameof(MobileSortingService.CurrentOption), _mobileSortingService.CurrentOption.ToString()
			},
			{
				nameof(MobileSortingService) + nameof(MobileSortingService.IsReversed), _mobileSortingService.IsReversed.ToString()
			}
		});

		UpdateVisibleRepositoryList(SearchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
	}

	void SetRepositoriesCollection(in IReadOnlyList<Repository> repositories, in string searchBarText)
	{
		_repositoryList = repositories;

		UpdateVisibleRepositoryList(searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
	}

	void AddRepositoriesToCollection(in IEnumerable<Repository> repositories, in string searchBarText, in bool shouldUpdateVisibleRepositoryList = true, Func<Repository, bool>? duplicateRepositoryPriorityFilter = null)
	{
		if (!repositories.Any())
			return;

		duplicateRepositoryPriorityFilter ??= _ => true;

		var updatedRepositoryList = _repositoryList.Concat(repositories);
		_repositoryList = [.. updatedRepositoryList.RemoveForksDuplicatesAndArchives(duplicateRepositoryPriorityFilter)];

		if (shouldUpdateVisibleRepositoryList)
			UpdateVisibleRepositoryList(searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
	}

	void RemoveRepositoriesFromCollection(in IEnumerable<Repository> repositories, in string searchBarText, in bool shouldUpdateVisibleRepositoryList = true)
	{
		if (!repositories.Any())
			return;

		var updatedRepositoryList = new List<Repository>(_repositoryList);
		foreach (var repositoryToRemove in repositories)
		{
			updatedRepositoryList.Remove(repositoryToRemove);
		}

		_repositoryList = [.. updatedRepositoryList.RemoveForksDuplicatesAndArchives(static x => x.ContainsViewsClonesStarsData)];

		if (shouldUpdateVisibleRepositoryList)
			UpdateVisibleRepositoryList(searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
	}

	void UpdateVisibleRepositoryList(in string searchBarText, in SortingOption sortingOption, in bool isReversed)
	{
		var filteredRepositoryList = GetRepositoriesFilteredBySearchBar(_repositoryList, searchBarText);

		VisibleRepositoryList = [.. MobileSortingService.SortRepositories(filteredRepositoryList, sortingOption, isReversed)];
	}

	void UpdateListForLoggedOutUser()
	{
		_repositoryList = [];
		UpdateVisibleRepositoryList(string.Empty, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
	}

	[RelayCommand]
	void SetSearchBarText(string text)
	{
		if (EqualityComparer<string>.Default.Equals(SearchBarText, text))
			return;
        SearchBarText = text;

		if (_repositoryList.Any())
			UpdateVisibleRepositoryList(SearchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
	}

	void HandlePreferredLanguageChanged(object? sender, string? e) => UpdateText();

	void UpdateText()
	{
		TitleText = PageTitles.RepositoryPage;
		TotalButtonText = RepositoryPageConstants.TOTAL;
	}

	// Work-around because ContentPage.OnAppearing does not fire after `ContentPage.PushModalAsync()`
	// Fixed in Xamarin.Forms v5.0.0-sr10 https://github.com/xamarin/Xamarin.Forms/commit/103ef3df7063e42851288c6977567193a74eaaaf
	void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e) => IsRefreshing |= e.IsSessionAuthorized;

	async void HandleShouldIncludeOrganizationsChanged(object? sender, bool e)
	{
		UpdateListForLoggedOutUser();
		await _repositoryDatabase.DeleteAllData(CancellationToken.None).ConfigureAwait(false);
	}

	void HandleRepositoryUriNotFound(object? sender, Uri e)
	{
		var repositoriesToRemove = _repositoryList.Where(x => x.Url == e.ToString());
		RemoveRepositoriesFromCollection(repositoriesToRemove, SearchBarText);
	}

	void HandleDemoUserActivated(object? sender, EventArgs e) => IsRefreshing = true;

	void HandleGitHubAuthenticationServiceLoggedOut(object? sender, EventArgs e) => UpdateListForLoggedOutUser();

	void HandleSortingOptionRequested(object? sender, SortingOption sortingOption) => SortRepositoriesCommand.Execute(sortingOption);

	void OnPullToRefreshFailed(PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs)
	{
		RefreshState = pullToRefreshFailedEventArgs switch
		{
			ErrorPullToRefreshEventArgs => RefreshState.Error,
			AbuseLimitPullToRefreshEventArgs => RefreshState.AbuseLimit,
			LoginExpiredPullToRefreshEventArgs => RefreshState.LoginExpired,
			MaximumApiRequestsReachedEventArgs => RefreshState.MaximumApiLimit,
			_ => throw new NotSupportedException()
		};

		_pullToRefreshFailedEventManager.RaiseEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
	}

	void HandleSearchBarTextChanged(object? sender, string searchBarText) => SetSearchBarText(searchBarText);

	void HandleScheduleRetryUpdatedRepositoriesStarsCompleted(object? sender, Repository e) => AddRepositoriesToCollection([e], SearchBarText, RefreshState is RefreshState.Succeeded or RefreshState.Uninitialized, x => x.ContainsViewsClonesStarsData);
	void HandleTrendsViewModelRepositorySavedToDatabase(object? sender, Repository e) => AddRepositoriesToCollection([e], SearchBarText, RefreshState is RefreshState.Succeeded or RefreshState.Uninitialized, x => x.ContainsViewsClonesStarsData);
	void HandleJobCompleted(object? sender, Repository e) => AddRepositoriesToCollection([e], SearchBarText, RefreshState is RefreshState.Succeeded or RefreshState.Uninitialized, x => x.ContainsViewsClonesStarsData);
}