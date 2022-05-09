using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
	public class TrendsViewModel : BaseViewModel
	{
		public const int MinimumChartHeight = 20;

		readonly static WeakEventManager<Repository> _repostoryEventManager = new();

		readonly GitHubApiV3Service _gitHubApiV3Service;
		readonly RepositoryDatabase _repositoryDatabase;
		readonly GitHubApiStatusService _gitHubApiStatusService;
		readonly BackgroundFetchService _backgroundFetchService;
		readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

		bool _isFetchingStarsData = true;
		bool _isFetchingViewsClonesData = true;

		bool _isViewsSeriesVisible, _isUniqueViewsSeriesVisible, _isClonesSeriesVisible, _isUniqueClonesSeriesVisible;

		string _starsStatisticsText = string.Empty;
		string _viewsStatisticsText = string.Empty;
		string _clonesStatisticsText = string.Empty;
		string _starsHeaderMessageText = string.Empty;
		string _uniqueViewsStatisticsText = string.Empty;
		string _uniqueClonesStatisticsText = string.Empty;
		string _starsEmptyDataViewTitleText = string.Empty;
		string _starsEmptyDataViewDescriptionText = string.Empty;
		string _viewsClonesEmptyDataViewTitleText = string.Empty;

		ImageSource? __starsEmptyDataViewImage;
		ImageSource? _viewsClonesEmptyDataViewImage;

		IReadOnlyList<DailyStarsModel>? _dailyStarsList;
		IReadOnlyList<DailyViewsModel>? _dailyViewsList;
		IReadOnlyList<DailyClonesModel>? _dailyClonesList;

		public TrendsViewModel(IMainThread mainThread,
								IAnalyticsService analyticsService,
								RepositoryDatabase repositoryDatabse,
								GitHubApiV3Service gitHubApiV3Service,
								GitHubApiStatusService gitHubApiStatusService,
								BackgroundFetchService backgroundFetchService,
								GitHubGraphQLApiService gitHubGraphQLApiService,
								TrendsChartSettingsService trendsChartSettingsService) : base(analyticsService, mainThread)
		{
			_gitHubApiV3Service = gitHubApiV3Service;
			_repositoryDatabase = repositoryDatabse;
			_gitHubApiStatusService = gitHubApiStatusService;
			_backgroundFetchService = backgroundFetchService;
			_gitHubGraphQLApiService = gitHubGraphQLApiService;

			IsViewsSeriesVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
			IsUniqueViewsSeriesVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
			IsClonesSeriesVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
			IsUniqueClonesSeriesVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;

			ViewsCardTappedCommand = new Command(() => IsViewsSeriesVisible = !IsViewsSeriesVisible);
			UniqueViewsCardTappedCommand = new Command(() => IsUniqueViewsSeriesVisible = !IsUniqueViewsSeriesVisible);
			ClonesCardTappedCommand = new Command(() => IsClonesSeriesVisible = !IsClonesSeriesVisible);
			UniqueClonesCardTappedCommand = new Command(() => IsUniqueClonesSeriesVisible = !IsUniqueClonesSeriesVisible);

			StarsRefreshState = ViewsClonesRefreshState = RefreshState.Uninitialized;

			FetchDataCommand = new AsyncCommand<(Repository Repository, CancellationToken CancellationToken)>(tuple => ExecuteFetchDataCommand(tuple.Repository, tuple.CancellationToken));
		}

		public ICommand ViewsCardTappedCommand { get; }
		public ICommand UniqueViewsCardTappedCommand { get; }
		public ICommand ClonesCardTappedCommand { get; }
		public ICommand UniqueClonesCardTappedCommand { get; }

		public static event EventHandler<Repository> RepositorySavedToDatabase
		{
			add => _repostoryEventManager.AddEventHandler(value);
			remove => _repostoryEventManager.RemoveEventHandler(value);
		}

		public IAsyncCommand<(Repository Repository, CancellationToken CancellationToken)> FetchDataCommand { get; }

		public double DailyViewsClonesMinValue { get; } = 0;
		public double MinDailyStarsValue { get; } = 0;

		public double TotalStars => DailyStarsList.Any() ? DailyStarsList.Last().TotalStars : 0;

		public bool IsStarsEmptyDataViewVisible => !IsStarsChartVisible && !IsFetchingStarsData;
		public bool IsStarsChartVisible => !IsFetchingStarsData && TotalStars > 1;

		public bool IsViewsClonesEmptyDataViewVisible => !IsViewsClonesChartVisible && !IsFetchingViewsClonesData;
		public bool IsViewsClonesChartVisible => !IsFetchingViewsClonesData && DailyViewsList.Sum(x => x.TotalViews + x.TotalUniqueViews) + DailyClonesList.Sum(x => x.TotalClones + x.TotalUniqueClones) > 0;

		public double ViewsClonesChartYAxisInterval => DailyViewsClonesMaxValue > 20 ? Math.Round(DailyViewsClonesMaxValue / 10) : 2;
		public double StarsChartYAxisInterval => MaxDailyStarsValue > 20 ? Math.Round(MaxDailyStarsValue / 10) : 2;

		public DateTime MinViewsClonesDate => DateTimeService.GetMinimumLocalDateTime(DailyViewsList, DailyClonesList);
		public DateTime MaxViewsClonesDate => DateTimeService.GetMaximumLocalDateTime(DailyViewsList, DailyClonesList);

		public DateTime MaxDailyStarsDate => DailyStarsList.Any() ? DailyStarsList.Last().LocalDay : DateTime.Today;
		public DateTime MinDailyStarsDate => DailyStarsList.Any() ? DailyStarsList.First().LocalDay : DateTime.Today.Subtract(TimeSpan.FromDays(14));

		public double MaxDailyStarsValue => TotalStars > MinimumChartHeight ? TotalStars : MinimumChartHeight;

		public double DailyViewsClonesMaxValue
		{
			get
			{
				var dailyViewMaxValue = DailyViewsList.Any() ? DailyViewsList.Max(x => x.TotalViews) : 0;
				var dailyClonesMaxValue = DailyClonesList.Any() ? DailyClonesList.Max(x => x.TotalClones) : 0;

				return Math.Max(Math.Max(dailyViewMaxValue, dailyClonesMaxValue), MinimumChartHeight);
			}
		}

		public ImageSource? ViewsClonesEmptyDataViewImage
		{
			get => _viewsClonesEmptyDataViewImage;
			set => SetProperty(ref _viewsClonesEmptyDataViewImage, value);
		}

		public ImageSource? StarsEmptyDataViewImage
		{
			get => __starsEmptyDataViewImage;
			set => SetProperty(ref __starsEmptyDataViewImage, value);
		}

		public string ViewsClonesEmptyDataViewTitleText
		{
			get => _viewsClonesEmptyDataViewTitleText;
			set => SetProperty(ref _viewsClonesEmptyDataViewTitleText, value);
		}

		public string StarsEmptyDataViewTitleText
		{
			get => _starsEmptyDataViewTitleText;
			set => SetProperty(ref _starsEmptyDataViewTitleText, value);
		}

		public string StarsEmptyDataViewDescriptionText
		{
			get => _starsEmptyDataViewDescriptionText;
			set => SetProperty(ref _starsEmptyDataViewDescriptionText, value);
		}

		public string StarsStatisticsText
		{
			get => _starsStatisticsText;
			set => SetProperty(ref _starsStatisticsText, value);
		}

		public string ViewsStatisticsText
		{
			get => _viewsStatisticsText;
			set => SetProperty(ref _viewsStatisticsText, value);
		}

		public string UniqueViewsStatisticsText
		{
			get => _uniqueViewsStatisticsText;
			set => SetProperty(ref _uniqueViewsStatisticsText, value);
		}

		public string ClonesStatisticsText
		{
			get => _clonesStatisticsText;
			set => SetProperty(ref _clonesStatisticsText, value);
		}

		public string UniqueClonesStatisticsText
		{
			get => _uniqueClonesStatisticsText;
			set => SetProperty(ref _uniqueClonesStatisticsText, value);
		}

		public bool IsViewsSeriesVisible
		{
			get => _isViewsSeriesVisible;
			set => SetProperty(ref _isViewsSeriesVisible, value);
		}

		public bool IsUniqueViewsSeriesVisible
		{
			get => _isUniqueViewsSeriesVisible;
			set => SetProperty(ref _isUniqueViewsSeriesVisible, value);
		}

		public bool IsClonesSeriesVisible
		{
			get => _isClonesSeriesVisible;
			set => SetProperty(ref _isClonesSeriesVisible, value);
		}

		public bool IsUniqueClonesSeriesVisible
		{
			get => _isUniqueClonesSeriesVisible;
			set => SetProperty(ref _isUniqueClonesSeriesVisible, value);
		}

		public string StarsHeaderMessageText
		{
			get => _starsHeaderMessageText;
			set => SetProperty(ref _starsHeaderMessageText, value);
		}

		public bool IsFetchingViewsClonesData
		{
			get => _isFetchingViewsClonesData;
			set => SetProperty(ref _isFetchingViewsClonesData, value, OnIsFetchingViewsClonesDataChanged);
		}

		public bool IsFetchingStarsData
		{
			get => _isFetchingStarsData;
			set => SetProperty(ref _isFetchingStarsData, value, OnIsFetchingStarsDataChanged);
		}

		public IReadOnlyList<DailyViewsModel> DailyViewsList
		{
			get => _dailyViewsList ??= Array.Empty<DailyViewsModel>();
			set => SetProperty(ref _dailyViewsList, value, OnDailyViewsListChanged);
		}

		public IReadOnlyList<DailyClonesModel> DailyClonesList
		{
			get => _dailyClonesList ??= Array.Empty<DailyClonesModel>();
			set => SetProperty(ref _dailyClonesList, value, OnDailyClonesListChanged);
		}

		public IReadOnlyList<DailyStarsModel> DailyStarsList
		{
			get => _dailyStarsList ??= Array.Empty<DailyStarsModel>();
			set => SetProperty(ref _dailyStarsList, value, OnDailyStarsListChanged);
		}

		RefreshState ViewsClonesRefreshState
		{
			set
			{
				ViewsClonesEmptyDataViewImage = EmptyDataViewService.GetViewsClonesImage(value);
				ViewsClonesEmptyDataViewTitleText = EmptyDataViewService.GetViewsClonesTitleText(value);
			}
		}

		RefreshState StarsRefreshState
		{
			set
			{
				StarsEmptyDataViewImage = EmptyDataViewService.GetStarsEmptyDataViewImage(value, TotalStars);
				StarsEmptyDataViewTitleText = EmptyDataViewService.GetStarsEmptyDataViewTitleText(value, TotalStars);
				StarsEmptyDataViewDescriptionText = EmptyDataViewService.GetStarsEmptyDataViewDescriptionText(value, TotalStars);

				StarsHeaderMessageText = EmptyDataViewService.GetStarsHeaderMessageText(value, TotalStars);
			}
		}

		async Task ExecuteFetchDataCommand(Repository repository, CancellationToken cancellationToken)
		{
			var minimumTimeTask = Task.Delay(TimeSpan.FromSeconds(1));
			using var getGetStarsDataCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			using var getViewsClonesDataCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			try
			{
				IReadOnlyList<DateTimeOffset> repositoryStars;
				IReadOnlyList<DailyViewsModel> repositoryViews;
				IReadOnlyList<DailyClonesModel> repositoryClones;

				var getGetStarsDataTask = isStarsDataComplete(repository) ? Task.FromResult(repository.StarredAt ?? throw new InvalidOperationException()) : GetStarsData(repository, getGetStarsDataCTS.Token);
				var getViewsClonesDataTask = isViewsClonesDataComplete(repository)
												? Task.FromResult((repository.DailyViewsList ?? throw new InvalidOperationException(), repository.DailyClonesList ?? throw new InvalidOperationException()))
												: GetViewsClonesData(repository, getViewsClonesDataCTS.Token);

				// Update Views Clones Data first because `GetViewsClonesData` is quicker than `GetStarsData`
				if (isViewsClonesDataComplete(repository))
				{
					repositoryViews = repository.DailyViewsList ?? throw new InvalidOperationException($"{nameof(Repository.DailyViewsList)} cannot be null when {nameof(Repository.ContainsViewsClonesStarsData)} is true");
					repositoryClones = repository.DailyClonesList ?? throw new InvalidOperationException($"{nameof(Repository.DailyClonesList)} cannot be null when {nameof(Repository.ContainsViewsClonesStarsData)} is true");

					updateViewsClonesData(repositoryViews, repositoryClones);
				}
				else
				{
					(repositoryViews, repositoryClones) = await getViewsClonesDataTask.ConfigureAwait(false);
					updateViewsClonesData(repositoryViews, repositoryClones);
				}

				//Set ViewsClonesRefreshState last, because EmptyDataViews are dependent on the Chart ItemSources, e.g. DailyViewsClonesList
				ViewsClonesRefreshState = RefreshState.Succeeded;
				IsFetchingViewsClonesData = false;

				if (isStarsDataComplete(repository))
				{
					repositoryStars = repository.StarredAt ?? throw new InvalidOperationException($"{nameof(Repository.StarredAt)} cannot be null when {nameof(Repository.ContainsViewsClonesStarsData)} is true");
					updateStarsData(repositoryStars);
				}
				else
				{
					var repositoryFromDatabase = await _repositoryDatabase.GetRepository(repository.Url).ConfigureAwait(false);

					if (repositoryFromDatabase is null)
					{
						repositoryStars = await getGetStarsDataTask.ConfigureAwait(false);
						updateStarsData(repositoryStars);
					}
					else
					{
						var estimatedRepositoryStars = getEstimatedStarredAtList(repositoryFromDatabase, repository.StarCount);
						updateStarsData(estimatedRepositoryStars);

						// Display the estimated Data
						StarsRefreshState = RefreshState.Succeeded;
						IsFetchingStarsData = false;

						// Continue to fetch the actual StarredAt Data in the background
						// This allows us to save the downlaoded data to the database at the end of the `try` block
						repositoryStars = await getGetStarsDataTask.ConfigureAwait(false);
						updateStarsData(repositoryStars);
					}
				}

				//Set StarsRefreshState last, because EmptyDataViews are dependent on the Chart ItemSources, e.g. DailyStarsList
				StarsRefreshState = RefreshState.Succeeded;
				IsFetchingStarsData = false;

				var updatedRepository = repository with
				{
					DataDownloadedAt = DateTimeOffset.UtcNow,
					StarredAt = repositoryStars,
					DailyViewsList = repositoryViews,
					DailyClonesList = repositoryClones
				};

				await _repositoryDatabase.SaveRepository(updatedRepository).ConfigureAwait(false);
				OnRepositorySavedToDatabase(updatedRepository);

				static bool isViewsClonesDataComplete(in Repository repository) => repository.ContainsViewsClonesData
																					&& repository.DataDownloadedAt > DateTimeOffset.Now.Subtract(CachedDataConstants.ViewsClonesCacheLifeSpan);

				static bool isStarsDataComplete(in Repository repository) => repository.ContainsStarsData
																				&& repository.DataDownloadedAt > DateTimeOffset.Now.Subtract(CachedDataConstants.StarsDataCacheLifeSpan);
			}
			catch (Exception e) when (e is ApiException { StatusCode: HttpStatusCode.Unauthorized })
			{
				var (repositoryStars, repositoryViews, repositoryClones) = await GetNewestRepositoryData(repository).ConfigureAwait(false);
				updateStarsData(repositoryStars);
				updateViewsClonesData(repositoryViews, repositoryClones);

				StarsRefreshState = ViewsClonesRefreshState = RefreshState.LoginExpired;
			}
			catch (Exception e) when (_gitHubApiStatusService.HasReachedMaximumApiCallLimit(e))
			{
				var (repositoryStars, repositoryViews, repositoryClones) = await GetNewestRepositoryData(repository).ConfigureAwait(false);
				updateStarsData(repositoryStars);
				updateViewsClonesData(repositoryViews, repositoryClones);

				StarsRefreshState = ViewsClonesRefreshState = RefreshState.MaximumApiLimit;
			}
			catch (Exception e) when (_gitHubApiStatusService.IsAbuseRateLimit(e, out var retryTimeSpan))
			{
				IReadOnlyList<DateTimeOffset> repositoryStars;
				IReadOnlyList<DailyViewsModel> repositoryViews;
				IReadOnlyList<DailyClonesModel> repositoryClones;

				_backgroundFetchService.TryScheduleRetryRepositoriesViewsClonesStars(repository, retryTimeSpan.Value);

				var repositoryFromDatabase = await _repositoryDatabase.GetRepository(repository.Url).ConfigureAwait(false);

				if (repositoryFromDatabase is null)
				{
					repositoryStars = Array.Empty<DateTimeOffset>();
					repositoryViews = Array.Empty<DailyViewsModel>();
					repositoryClones = Array.Empty<DailyClonesModel>();

					StarsRefreshState = ViewsClonesRefreshState = RefreshState.Error;
				}
				else if (repositoryFromDatabase.DataDownloadedAt > repository.DataDownloadedAt) //If data from database is more recent, display data from database
				{
					var estimatedRepositoryStars = getEstimatedStarredAtList(repositoryFromDatabase, repository.StarCount);

					repositoryStars = estimatedRepositoryStars;
					repositoryViews = repositoryFromDatabase.DailyViewsList ?? Array.Empty<DailyViewsModel>();
					repositoryClones = repositoryFromDatabase.DailyClonesList ?? Array.Empty<DailyClonesModel>();

					StarsRefreshState = ViewsClonesRefreshState = RefreshState.Succeeded;
				}
				else //If data passed in as parameter is more recent, display data passed in as parameter
				{
					repositoryStars = repository.StarredAt ?? Array.Empty<DateTimeOffset>();
					repositoryViews = repository.DailyViewsList ?? Array.Empty<DailyViewsModel>();
					repositoryClones = repository.DailyClonesList ?? Array.Empty<DailyClonesModel>();

					StarsRefreshState = ViewsClonesRefreshState = RefreshState.Succeeded;
				}

				updateStarsData(repositoryStars);
				updateViewsClonesData(repositoryViews, repositoryClones);
			}
			catch (Exception e)
			{
				AnalyticsService.Report(e);

				var (repositoryStars, repositoryViews, repositoryClones) = await GetNewestRepositoryData(repository).ConfigureAwait(false);
				updateStarsData(repositoryStars);
				updateViewsClonesData(repositoryViews, repositoryClones);

				StarsRefreshState = ViewsClonesRefreshState = RefreshState.Error;
			}
			finally
			{
				if (!getGetStarsDataCTS.IsCancellationRequested)
					getGetStarsDataCTS.Cancel();

				if (!getViewsClonesDataCTS.IsCancellationRequested)
					getViewsClonesDataCTS.Cancel();

				//Display the Activity Indicator for a minimum time to ensure consistant UX
				await minimumTimeTask.ConfigureAwait(false);
				IsFetchingStarsData = IsFetchingViewsClonesData = false;
			}

			PrintDays();

			void updateViewsClonesData(in IEnumerable<DailyViewsModel> repositoryViews, in IEnumerable<DailyClonesModel> repositoryClones)
			{
				DailyViewsList = repositoryViews.OrderBy(x => x.Day).ToList();
				DailyClonesList = repositoryClones.OrderBy(x => x.Day).ToList();

				ViewsStatisticsText = repositoryViews.Sum(x => x.TotalViews).ToAbbreviatedText();
				UniqueViewsStatisticsText = repositoryViews.Sum(x => x.TotalUniqueViews).ToAbbreviatedText();

				ClonesStatisticsText = repositoryClones.Sum(x => x.TotalClones).ToAbbreviatedText();
				UniqueClonesStatisticsText = repositoryClones.Sum(x => x.TotalUniqueClones).ToAbbreviatedText();
			}

			void updateStarsData(in IReadOnlyList<DateTimeOffset> repositoryStars)
			{
				DailyStarsList = GetDailyStarsList(repositoryStars).OrderBy(x => x.Day).ToList();
				StarsStatisticsText = repositoryStars.Count.ToAbbreviatedText();
			}

			IReadOnlyList<DateTimeOffset> getEstimatedStarredAtList(in Repository repositoryFromDatabase, in long starCount)
			{
				if (starCount is 0)
					return Array.Empty<DateTimeOffset>();

				var incompleteStarredAtList = new List<DateTimeOffset>(repositoryFromDatabase.StarredAt ?? new List<DateTimeOffset> { DateTimeOffset.MinValue });
				var totalMissingTime = DateTimeOffset.UtcNow.Subtract(incompleteStarredAtList.Last());
				var missingStarCount = starCount - incompleteStarredAtList.Count;

				for (var i = 1; i <= missingStarCount; i++)
				{
					var nextDataPointDeltaInSeconds = totalMissingTime.TotalSeconds / missingStarCount * i;
					incompleteStarredAtList.Add(incompleteStarredAtList.Last().AddSeconds(nextDataPointDeltaInSeconds));
				}

				return incompleteStarredAtList;
			}
		}

		async Task<(IReadOnlyList<DateTimeOffset> RepositoryStars, IReadOnlyList<DailyViewsModel> RepositoryViews, IReadOnlyList<DailyClonesModel> RepositoryClones)>
			GetNewestRepositoryData(Repository repository)
		{
			IReadOnlyList<DateTimeOffset> repositoryStars;
			IReadOnlyList<DailyViewsModel> repositoryViews;
			IReadOnlyList<DailyClonesModel> repositoryClones;

			var repositoryFromDatabase = await _repositoryDatabase.GetRepository(repository.Url).ConfigureAwait(false);

			if (repositoryFromDatabase is null)
			{
				repositoryStars = Array.Empty<DateTimeOffset>();
				repositoryViews = Array.Empty<DailyViewsModel>();
				repositoryClones = Array.Empty<DailyClonesModel>();
			}
			else if (repositoryFromDatabase.DataDownloadedAt > repository.DataDownloadedAt) //If data from database is more recent, display data from database
			{
				repositoryStars = repositoryFromDatabase.StarredAt ?? Array.Empty<DateTimeOffset>();
				repositoryViews = repositoryFromDatabase.DailyViewsList ?? Array.Empty<DailyViewsModel>();
				repositoryClones = repositoryFromDatabase.DailyClonesList ?? Array.Empty<DailyClonesModel>();
			}
			else //If data passed in as parameter is more recent, display data passed in as parameter
			{
				repositoryStars = repository.StarredAt ?? Array.Empty<DateTimeOffset>();
				repositoryViews = repository.DailyViewsList ?? Array.Empty<DailyViewsModel>();
				repositoryClones = repository.DailyClonesList ?? Array.Empty<DailyClonesModel>();
			}

			return (repositoryStars, repositoryViews, repositoryClones);
		}


		IEnumerable<DailyStarsModel> GetDailyStarsList(IReadOnlyList<DateTimeOffset> starredAtDates)
		{
			int totalStars = 0;

			foreach (var starDate in starredAtDates)
				yield return new DailyStarsModel(++totalStars, starDate);

			//Ensure chart includes todays date
			if (starredAtDates.Any() && starredAtDates.Max().DayOfYear != DateTimeOffset.UtcNow.DayOfYear)
				yield return new DailyStarsModel(totalStars, DateTimeOffset.UtcNow);
		}

		async Task<IReadOnlyList<DateTimeOffset>> GetStarsData(Repository repository, CancellationToken cancellationToken)
		{
			if (isFetchingStarsInBackground(_backgroundFetchService, repository))
			{
				return await getRepositoryStarsFromBackgroundService(repository, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				var getStarGazers = await _gitHubGraphQLApiService.GetStarGazers(repository.Name, repository.OwnerLogin, cancellationToken).ConfigureAwait(false);
				return getStarGazers.StarredAt.Select(x => x.StarredAt).ToList();
			}

			static bool isFetchingStarsInBackground(BackgroundFetchService backgroundFetchService, Repository repository) =>
				backgroundFetchService.QueuedJobs.Any(x => x == backgroundFetchService.GetRetryRepositoriesStarsIdentifier(repository));

			async Task<IReadOnlyList<DateTimeOffset>> getRepositoryStarsFromBackgroundService(Repository repository, CancellationToken cancellationToken)
			{
				var backgroundStarsTCS = new TaskCompletionSource<IReadOnlyList<DateTimeOffset>>();

				BackgroundFetchService.ScheduleRetryRepositoriesStarsCompleted += HandleScheduleRetryRepositoriesStarsCompleted;

				using var cancellationTokenRegistration = cancellationToken.Register(() =>
				{
					BackgroundFetchService.ScheduleRetryRepositoriesStarsCompleted -= HandleScheduleRetryRepositoriesStarsCompleted;
					backgroundStarsTCS.SetCanceled();
				}); // Work-around to use a CancellationToken with a TaskCompletionSource: https://stackoverflow.com/a/39897392/5953643

				return await backgroundStarsTCS.Task.ConfigureAwait(false);

				void HandleScheduleRetryRepositoriesStarsCompleted(object sender, Repository e)
				{
					if (e.Url == repository.Url)
					{
						BackgroundFetchService.ScheduleRetryRepositoriesStarsCompleted -= HandleScheduleRetryRepositoriesStarsCompleted;
						backgroundStarsTCS.SetResult(e.StarredAt ?? throw new InvalidOperationException($"{nameof(e.StarredAt)} cannot be null"));
					}
				}
			}
		}

		async Task<(IReadOnlyList<DailyViewsModel> RepositoryViews, IReadOnlyList<DailyClonesModel> RepositoryClones)> GetViewsClonesData(Repository repository, CancellationToken cancellationToken)
		{
			if (isFetchingViewsClonesStarsInBackground(_backgroundFetchService, repository))
			{
				var backgroundServiceResults = await getRepositoryViewsClonesStarsFromBackgroundService(repository, cancellationToken).ConfigureAwait(false);
				return (backgroundServiceResults.RepositoryViews, backgroundServiceResults.RepositoryClones);
			}
			else
			{
				var getRepositoryViewStatisticsTask = _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name, cancellationToken);
				var getRepositoryCloneStatisticsTask = _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name, cancellationToken);

				await Task.WhenAll(getRepositoryViewStatisticsTask, getRepositoryCloneStatisticsTask).ConfigureAwait(false);

				var repositoryViewsResponse = await getRepositoryViewStatisticsTask.ConfigureAwait(false);
				var repositoryClonesResponse = await getRepositoryCloneStatisticsTask.ConfigureAwait(false);

				return (repositoryViewsResponse.DailyViewsList, repositoryClonesResponse.DailyClonesList);
			}

			static bool isFetchingViewsClonesStarsInBackground(BackgroundFetchService backgroundFetchService, Repository repository) =>
						backgroundFetchService.QueuedJobs.Any(x => x == backgroundFetchService.GetRetryRepositoriesViewsClonesStarsIdentifier(repository));

			async Task<(IReadOnlyList<DateTimeOffset> RepositoryStars, IReadOnlyList<DailyViewsModel> RepositoryViews, IReadOnlyList<DailyClonesModel> RepositoryClones)> getRepositoryViewsClonesStarsFromBackgroundService(Repository repository, CancellationToken cancellationToken)
			{
				var backgroundStarsTCS = new TaskCompletionSource<(IReadOnlyList<DateTimeOffset> RepositoryStars, IReadOnlyList<DailyViewsModel> RepositoryViews, IReadOnlyList<DailyClonesModel> RepositoryClones)>();

				BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted += HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;

				using var cancellationTokenRegistration = cancellationToken.Register(() =>
				{
					BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted -= HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
					backgroundStarsTCS.SetCanceled();
				}); // Work-around to use a CancellationToken with a TaskCompletionSource: https://stackoverflow.com/a/39897392/5953643

				return await backgroundStarsTCS.Task.ConfigureAwait(false);

				void HandleScheduleRetryRepositoriesViewsClonesStarsCompleted(object sender, Repository e)
				{
					if (e.Url == repository.Url)
					{
						BackgroundFetchService.ScheduleRetryRepositoriesViewsClonesStarsCompleted -= HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
						backgroundStarsTCS.SetResult((e.StarredAt ?? throw new InvalidOperationException($"{nameof(e.StarredAt)} cannot be null"),
														e.DailyViewsList ?? throw new InvalidOperationException($"{nameof(e.DailyViewsList)} cannot be null"),
														e.DailyClonesList ?? throw new InvalidOperationException($"{nameof(e.DailyClonesList)} cannot be null")));
					}
				}
			}
		}

		void OnDailyStarsListChanged()
		{
			OnPropertyChanged(nameof(IsStarsChartVisible));
			OnPropertyChanged(nameof(IsStarsEmptyDataViewVisible));

			OnPropertyChanged(nameof(MaxDailyStarsValue));
			OnPropertyChanged(nameof(MinDailyStarsValue));

			OnPropertyChanged(nameof(MaxDailyStarsDate));
			OnPropertyChanged(nameof(MinDailyStarsDate));

			OnPropertyChanged(nameof(TotalStars));

			OnPropertyChanged(nameof(StarsChartYAxisInterval));
		}

		void OnDailyClonesListChanged()
		{
			OnPropertyChanged(nameof(IsViewsClonesChartVisible));
			OnPropertyChanged(nameof(IsViewsClonesEmptyDataViewVisible));

			OnPropertyChanged(nameof(DailyViewsClonesMaxValue));
			OnPropertyChanged(nameof(DailyViewsClonesMinValue));

			OnPropertyChanged(nameof(MinViewsClonesDate));
			OnPropertyChanged(nameof(MaxViewsClonesDate));

			OnPropertyChanged(nameof(ViewsClonesChartYAxisInterval));
		}

		void OnDailyViewsListChanged()
		{
			OnPropertyChanged(nameof(IsViewsClonesChartVisible));
			OnPropertyChanged(nameof(IsViewsClonesEmptyDataViewVisible));

			OnPropertyChanged(nameof(DailyViewsClonesMaxValue));
			OnPropertyChanged(nameof(DailyViewsClonesMaxValue));

			OnPropertyChanged(nameof(MinViewsClonesDate));
			OnPropertyChanged(nameof(MaxViewsClonesDate));

			OnPropertyChanged(nameof(ViewsClonesChartYAxisInterval));
		}

		void OnIsFetchingViewsClonesDataChanged()
		{
			OnPropertyChanged(nameof(IsViewsClonesChartVisible));
			OnPropertyChanged(nameof(IsViewsClonesEmptyDataViewVisible));
		}

		void OnIsFetchingStarsDataChanged()
		{
			OnPropertyChanged(nameof(IsStarsChartVisible));
			OnPropertyChanged(nameof(IsStarsEmptyDataViewVisible));
		}

		void OnRepositorySavedToDatabase(in Repository repository) => _repostoryEventManager.RaiseEvent(this, repository, nameof(RepositorySavedToDatabase));

		[Conditional("DEBUG")]
		void PrintDays()
		{
			Debug.WriteLine("Clones");
			foreach (var cloneDay in DailyClonesList.Select(x => x.Day))
				Debug.WriteLine(cloneDay);

			Debug.WriteLine("");

			Debug.WriteLine("Views");
			foreach (var viewDay in DailyViewsList.Select(x => x.Day))
				Debug.WriteLine(viewDay);
		}
	}
}