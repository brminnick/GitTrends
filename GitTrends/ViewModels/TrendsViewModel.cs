using System.Diagnostics;
using System.Net;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHubApiStatus;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using Refit;

namespace GitTrends;

public partial class TrendsViewModel : BaseViewModel, IQueryAttributable
{
    public const int MinimumChartHeight = 20;
    public const string RepositoryQueryString = nameof(RepositoryQueryString);

    static readonly WeakEventManager<Repository> _repositoryEventManager = new();

    readonly GitHubApiV3Service _gitHubApiV3Service;
    readonly RepositoryDatabase _repositoryDatabase;
    readonly IGitHubApiStatusService _gitHubApiStatusService;
    readonly BackgroundFetchService _backgroundFetchService;
    readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

    public TrendsViewModel(IDispatcher dispatcher,
        IAnalyticsService analyticsService,
        RepositoryDatabase repositoryDatabase,
        GitHubApiV3Service gitHubApiV3Service,
        IGitHubApiStatusService gitHubApiStatusService,
        BackgroundFetchService backgroundFetchService,
        GitHubGraphQLApiService gitHubGraphQLApiService,
        TrendsChartSettingsService trendsChartSettingsService) : base(analyticsService, dispatcher)
    {
        _gitHubApiV3Service = gitHubApiV3Service;
        _repositoryDatabase = repositoryDatabase;
        _gitHubApiStatusService = gitHubApiStatusService;
        _backgroundFetchService = backgroundFetchService;
        _gitHubGraphQLApiService = gitHubGraphQLApiService;

        IsViewsSeriesVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
        IsUniqueViewsSeriesVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
        IsClonesSeriesVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
        IsUniqueClonesSeriesVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;

        ViewsCardTappedCommand = new RelayCommand(() => IsViewsSeriesVisible = !IsViewsSeriesVisible);
        UniqueViewsCardTappedCommand = new RelayCommand(() => IsUniqueViewsSeriesVisible = !IsUniqueViewsSeriesVisible);
        ClonesCardTappedCommand = new RelayCommand(() => IsClonesSeriesVisible = !IsClonesSeriesVisible);
        UniqueClonesCardTappedCommand =
            new RelayCommand(() => IsUniqueClonesSeriesVisible = !IsUniqueClonesSeriesVisible);

        StarsRefreshState = ViewsClonesRefreshState = RefreshState.Uninitialized;
    }

    public ICommand ViewsCardTappedCommand { get; }
    public ICommand UniqueViewsCardTappedCommand { get; }
    public ICommand ClonesCardTappedCommand { get; }
    public ICommand UniqueClonesCardTappedCommand { get; }
    
        [ObservableProperty,
     NotifyPropertyChangedFor(nameof(IsStarsChartVisible)),
     NotifyPropertyChangedFor(nameof(IsStarsEmptyDataViewVisible))]
    public partial bool IsFetchingStarsData { get; private set; } = true;

    [ObservableProperty,
     NotifyPropertyChangedFor(nameof(IsViewsClonesChartVisible)),
     NotifyPropertyChangedFor(nameof(IsViewsClonesEmptyDataViewVisible))]
    public partial bool IsFetchingViewsClonesData { get; private set; } = true;

    [ObservableProperty]
    public partial bool IsViewsSeriesVisible { get; private set; }

    [ObservableProperty]
    public partial bool IsUniqueViewsSeriesVisible { get; private set; }

    [ObservableProperty]
    public partial bool IsClonesSeriesVisible { get; private set; }

    [ObservableProperty]
    public partial bool IsUniqueClonesSeriesVisible { get; private set; }

    [ObservableProperty]
    public partial string StarsStatisticsText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string ViewsStatisticsText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string ClonesStatisticsText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string StarsHeaderMessageText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string UniqueViewsStatisticsText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string UniqueClonesStatisticsText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string StarsEmptyDataViewTitleText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string StarsEmptyDataViewDescriptionText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string ViewsClonesEmptyDataViewTitleText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial ImageSource? StarsEmptyDataViewImage { get; private set; }

    [ObservableProperty]
    public partial ImageSource? ViewsClonesEmptyDataViewImage { get; private set; }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsViewsClonesChartVisible)),
     NotifyPropertyChangedFor(nameof(IsViewsClonesEmptyDataViewVisible)),
     NotifyPropertyChangedFor(nameof(DailyViewsClonesMaxValue)),
     NotifyPropertyChangedFor(nameof(DailyViewsClonesMaxValue)), NotifyPropertyChangedFor(nameof(MinViewsClonesDate)),
     NotifyPropertyChangedFor(nameof(MaxViewsClonesDate)),
     NotifyPropertyChangedFor(nameof(ViewsClonesChartYAxisInterval))]
    public partial IReadOnlyList<DailyViewsModel> DailyViewsList { get; private set; } = [];

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsViewsClonesChartVisible)),
     NotifyPropertyChangedFor(nameof(IsViewsClonesEmptyDataViewVisible)),
     NotifyPropertyChangedFor(nameof(DailyViewsClonesMaxValue)),
     NotifyPropertyChangedFor(nameof(DailyViewsClonesMinValue)), NotifyPropertyChangedFor(nameof(MinViewsClonesDate)),
     NotifyPropertyChangedFor(nameof(MaxViewsClonesDate)),
     NotifyPropertyChangedFor(nameof(ViewsClonesChartYAxisInterval))]
    public partial IReadOnlyList<DailyClonesModel> DailyClonesList { get; private set; } = [];

    [ObservableProperty,
     NotifyPropertyChangedFor(nameof(IsStarsChartVisible)),
     NotifyPropertyChangedFor(nameof(IsStarsEmptyDataViewVisible)),
     NotifyPropertyChangedFor(nameof(MaxDailyStarsValue)),
     NotifyPropertyChangedFor(nameof(MinDailyStarsValue)),
     NotifyPropertyChangedFor(nameof(MaxDailyStarsDate)),
     NotifyPropertyChangedFor(nameof(MinDailyStarsDate)),
     NotifyPropertyChangedFor(nameof(TotalStars)),
     NotifyPropertyChangedFor(nameof(StarsChartYAxisInterval))]
    public partial IReadOnlyList<DailyStarsModel> DailyStarsList { get; private set; } = [];

    public static event EventHandler<Repository> RepositorySavedToDatabase
    {
        add => _repositoryEventManager.AddEventHandler(value);
        remove => _repositoryEventManager.RemoveEventHandler(value);
    }

    public double DailyViewsClonesMinValue { get; } = 0;
    public double MinDailyStarsValue { get; } = 0;

    public double TotalStars => DailyStarsList.Count is not 0 ? DailyStarsList.Last().TotalStars : 0;

    public bool IsStarsEmptyDataViewVisible => !IsStarsChartVisible && !IsFetchingStarsData;
    public bool IsStarsChartVisible => !IsFetchingStarsData && TotalStars > 1;

    public bool IsViewsClonesEmptyDataViewVisible => !IsViewsClonesChartVisible && !IsFetchingViewsClonesData;

    public bool IsViewsClonesChartVisible => !IsFetchingViewsClonesData &&
                                             DailyViewsList.Sum(static x => x.TotalViews + x.TotalUniqueViews) +
                                             DailyClonesList.Sum(static x => x.TotalClones + x.TotalUniqueClones) > 0;

    public double ViewsClonesChartYAxisInterval =>
        DailyViewsClonesMaxValue > 20 ? Math.Round(DailyViewsClonesMaxValue / 10) : 2;

    public double StarsChartYAxisInterval => MaxDailyStarsValue > 20 ? Math.Round(MaxDailyStarsValue / 10) : 2;

    public DateTime MinViewsClonesDate => DateTimeService.GetMinimumLocalDateTime(DailyViewsList, DailyClonesList);
    public DateTime MaxViewsClonesDate => DateTimeService.GetMaximumLocalDateTime(DailyViewsList, DailyClonesList);

    public DateTime MaxDailyStarsDate =>
        DailyStarsList.Count is not 0 ? DailyStarsList.Last().LocalDay : DateTime.Today;

    public DateTime MinDailyStarsDate => DailyStarsList.Count is not 0
        ? DailyStarsList.First().LocalDay
        : DateTime.Today.Subtract(TimeSpan.FromDays(14));

    public double MaxDailyStarsValue => TotalStars > MinimumChartHeight ? TotalStars : MinimumChartHeight;

    public double DailyViewsClonesMaxValue
    {
        get
        {
            var dailyViewMaxValue = DailyViewsList.Any() ? DailyViewsList.Max(static x => x.TotalViews) : 0;
            var dailyClonesMaxValue = DailyClonesList.Any() ? DailyClonesList.Max(static x => x.TotalClones) : 0;

            return Math.Max(Math.Max(dailyViewMaxValue, dailyClonesMaxValue), MinimumChartHeight);
        }
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
            StarsEmptyDataViewDescriptionText =
                EmptyDataViewService.GetStarsEmptyDataViewDescriptionText(value, TotalStars);

            StarsHeaderMessageText = EmptyDataViewService.GetStarsHeaderMessageText(value, TotalStars);
        }
    }

    static IEnumerable<DailyStarsModel> GetDailyStarsList(IReadOnlyList<DateTimeOffset> starredAtDates)
    {
        int totalStars = 0;

        foreach (var starDate in starredAtDates)
            yield return new DailyStarsModel(++totalStars, starDate);

        //Ensure chart includes todays date
        if (starredAtDates.Any() && starredAtDates.Max().LocalDateTime.DayOfYear !=
            DateTimeOffset.UtcNow.LocalDateTime.DayOfYear)
            yield return new DailyStarsModel(totalStars, DateTimeOffset.UtcNow);
    }

    public async Task FetchData(Repository repository, CancellationToken cancellationToken)
    {
        var minimumTimeTask = Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        using var getGetStarsDataCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var getViewsClonesDataCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            IReadOnlyList<DateTimeOffset> repositoryStars;
            IReadOnlyList<DailyViewsModel> repositoryViews;
            IReadOnlyList<DailyClonesModel> repositoryClones;

            var getGetStarsDataTask = isStarsDataComplete(repository)
                ? Task.FromResult(repository.StarredAt ?? throw new InvalidOperationException())
                : GetStarsData(repository, getGetStarsDataCTS.Token);
            var getViewsClonesDataTask = isViewsClonesDataComplete(repository)
                ? Task.FromResult((repository.DailyViewsList ?? throw new InvalidOperationException(),
                    repository.DailyClonesList ?? throw new InvalidOperationException()))
                : GetViewsClonesData(repository, getViewsClonesDataCTS.Token);

            // Update Views Clones Data first because `GetViewsClonesData` is quicker than `GetStarsData`
            if (isViewsClonesDataComplete(repository))
            {
                repositoryViews = repository.DailyViewsList ?? throw new InvalidOperationException(
                    $"{nameof(Repository.DailyViewsList)} cannot be null when {nameof(Repository.ContainsViewsClonesStarsData)} is true");
                repositoryClones = repository.DailyClonesList ?? throw new InvalidOperationException(
                    $"{nameof(Repository.DailyClonesList)} cannot be null when {nameof(Repository.ContainsViewsClonesStarsData)} is true");

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
                repositoryStars = repository.StarredAt ?? throw new InvalidOperationException(
                    $"{nameof(Repository.StarredAt)} cannot be null when {nameof(Repository.ContainsViewsClonesStarsData)} is true");
                updateStarsData(repositoryStars);
            }
            else
            {
                var repositoryFromDatabase = await _repositoryDatabase.GetRepository(repository.Url, cancellationToken)
                    .ConfigureAwait(false);

                if (repositoryFromDatabase is not null && repositoryFromDatabase.StarredAt?.Count is not 0)
                {
                    var estimatedRepositoryStars =
                        DateTimeService.GetEstimatedStarredAtList(repositoryFromDatabase, repository.StarCount);
                    updateStarsData(estimatedRepositoryStars);

                    // Display the estimated Data
                    StarsRefreshState = RefreshState.Succeeded;
                    IsFetchingStarsData = false;
                }

                // Continue to fetch the actual StarredAt Data in the background
                // This allows us to save the downloaded data to the database at the end of the `try` block
                repositoryStars = await getGetStarsDataTask.ConfigureAwait(false);
                updateStarsData(repositoryStars);
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

            await _repositoryDatabase.SaveRepository(updatedRepository, cancellationToken).ConfigureAwait(false);
            OnRepositorySavedToDatabase(updatedRepository);

            static bool isViewsClonesDataComplete(in Repository repository) => repository.ContainsViewsClonesData
                                                                               && repository.DataDownloadedAt >
                                                                               DateTimeOffset.Now.Subtract(
                                                                                   CachedDataConstants
                                                                                       .ViewsClonesCacheLifeSpan);

            static bool isStarsDataComplete(in Repository repository) => repository.ContainsStarsData
                                                                         && repository.DataDownloadedAt >
                                                                         DateTimeOffset.Now.Subtract(CachedDataConstants
                                                                             .StarsDataCacheLifeSpan)
                                                                         && repository.StarredAt?.Count ==
                                                                         repository.StarCount;
        }
        catch (Exception e) when (e is ApiException { StatusCode: HttpStatusCode.Unauthorized })
        {
            var (repositoryStars, repositoryViews, repositoryClones) =
                await GetNewestRepositoryData(repository, cancellationToken).ConfigureAwait(false);
            updateStarsData(repositoryStars);
            updateViewsClonesData(repositoryViews, repositoryClones);

            StarsRefreshState = ViewsClonesRefreshState = RefreshState.LoginExpired;
        }
        catch (Exception e) when (_gitHubApiStatusService.HasReachedMaximumApiCallLimit(e))
        {
            var (repositoryStars, repositoryViews, repositoryClones) =
                await GetNewestRepositoryData(repository, cancellationToken).ConfigureAwait(false);
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

            var repositoryFromDatabase = await _repositoryDatabase.GetRepository(repository.Url, cancellationToken)
                .ConfigureAwait(false);

            if (repositoryFromDatabase is null)
            {
                repositoryStars = [];
                repositoryViews = [];
                repositoryClones = [];

                StarsRefreshState = ViewsClonesRefreshState = RefreshState.Error;
            }
            else if (repositoryFromDatabase.DataDownloadedAt >
                     repository.DataDownloadedAt) //If data from database is more recent, display data from database
            {
                var estimatedRepositoryStars =
                    DateTimeService.GetEstimatedStarredAtList(repositoryFromDatabase, repository.StarCount);

                repositoryStars = estimatedRepositoryStars;
                repositoryViews = repositoryFromDatabase.DailyViewsList ?? [];
                repositoryClones = repositoryFromDatabase.DailyClonesList ?? [];

                StarsRefreshState = ViewsClonesRefreshState = RefreshState.Succeeded;
            }
            else //If data passed in as parameter is more recent, display data passed in as parameter
            {
                repositoryStars = repository.StarredAt ?? [];
                repositoryViews = repository.DailyViewsList ?? [];
                repositoryClones = repository.DailyClonesList ?? [];

                StarsRefreshState = ViewsClonesRefreshState = RefreshState.Succeeded;
            }

            updateStarsData(repositoryStars);
            updateViewsClonesData(repositoryViews, repositoryClones);
        }
        catch (Exception e)
        {
            AnalyticsService.Report(e);

            var (repositoryStars, repositoryViews, repositoryClones) =
                await GetNewestRepositoryData(repository, cancellationToken).ConfigureAwait(false);
            updateStarsData(repositoryStars);
            updateViewsClonesData(repositoryViews, repositoryClones);

            StarsRefreshState = ViewsClonesRefreshState = RefreshState.Error;
        }
        finally
        {
            if (!getGetStarsDataCTS.IsCancellationRequested)
                await getGetStarsDataCTS.CancelAsync().ConfigureAwait(false);

            if (!getViewsClonesDataCTS.IsCancellationRequested)
                await getViewsClonesDataCTS.CancelAsync().ConfigureAwait(false);

            //Display the Activity Indicator for a minimum time to ensure consistent UX
            await minimumTimeTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.None);
            IsFetchingStarsData = IsFetchingViewsClonesData = false;
        }

        PrintDays();

        void updateViewsClonesData(in IEnumerable<DailyViewsModel> repositoryViews,
            in IEnumerable<DailyClonesModel> repositoryClones)
        {
            DailyViewsList = [.. repositoryViews.OrderBy(static x => x.Day)];
            DailyClonesList = [.. repositoryClones.OrderBy(static x => x.Day)];

            ViewsStatisticsText = repositoryViews.Sum(static x => x.TotalViews).ToAbbreviatedText();
            UniqueViewsStatisticsText = repositoryViews.Sum(static x => x.TotalUniqueViews).ToAbbreviatedText();

            ClonesStatisticsText = repositoryClones.Sum(static x => x.TotalClones).ToAbbreviatedText();
            UniqueClonesStatisticsText = repositoryClones.Sum(static x => x.TotalUniqueClones).ToAbbreviatedText();
        }

        void updateStarsData(in IReadOnlyList<DateTimeOffset> repositoryStars)
        {
            DailyStarsList = [.. GetDailyStarsList(repositoryStars).OrderBy(static x => x.Day)];
            StarsStatisticsText = repositoryStars.Count.ToAbbreviatedText();
        }
    }

    async Task<(IReadOnlyList<DateTimeOffset> RepositoryStars, IReadOnlyList<DailyViewsModel> RepositoryViews,
            IReadOnlyList<DailyClonesModel> RepositoryClones)>
        GetNewestRepositoryData(Repository repository, CancellationToken token)
    {
        IReadOnlyList<DateTimeOffset> repositoryStars;
        IReadOnlyList<DailyViewsModel> repositoryViews;
        IReadOnlyList<DailyClonesModel> repositoryClones;

        Repository? repositoryFromDatabase = null;

        try
        {
            repositoryFromDatabase =
                await _repositoryDatabase.GetRepository(repository.Url, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException e)
        {
            AnalyticsService.Report(e);
        }

        if (repositoryFromDatabase is null)
        {
            repositoryStars = [];
            repositoryViews = [];
            repositoryClones = [];
        }
        else if (repositoryFromDatabase.DataDownloadedAt >
                 repository.DataDownloadedAt) //If data from database is more recent, display data from database
        {
            repositoryStars = repositoryFromDatabase.StarredAt ?? [];
            repositoryViews = repositoryFromDatabase.DailyViewsList ?? [];
            repositoryClones = repositoryFromDatabase.DailyClonesList ?? [];
        }
        else //If data passed in as parameter is more recent, display data passed in as parameter
        {
            repositoryStars = repository.StarredAt ?? [];
            repositoryViews = repository.DailyViewsList ?? [];
            repositoryClones = repository.DailyClonesList ?? [];
        }

        return (repositoryStars, repositoryViews, repositoryClones);
    }

    async Task<IReadOnlyList<DateTimeOffset>> GetStarsData(Repository repository, CancellationToken cancellationToken)
    {
        if (_backgroundFetchService.IsFetchingStarsInBackground(repository))
        {
            return await getRepositoryStarsFromBackgroundService(repository, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var getStarGazers = await _gitHubGraphQLApiService
                .GetStarGazers(repository.Name, repository.OwnerLogin, cancellationToken).ConfigureAwait(false);
            return [.. getStarGazers.StarredAt.Select(static x => x.StarredAt)];
        }

        async Task<IReadOnlyList<DateTimeOffset>> getRepositoryStarsFromBackgroundService(Repository repository,
            CancellationToken cancellationToken)
        {
            var backgroundStarsTCS = new TaskCompletionSource<IReadOnlyList<DateTimeOffset>>();

            RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase += HandleScheduleRetryRepositoriesStarsCompleted;

            await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            {
                RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase -=
                    HandleScheduleRetryRepositoriesStarsCompleted;
                backgroundStarsTCS.SetCanceled(cancellationToken);
            }); // Work-around to use a CancellationToken with a TaskCompletionSource: https://stackoverflow.com/a/39897392/5953643

            return await backgroundStarsTCS.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

            void HandleScheduleRetryRepositoriesStarsCompleted(object? sender, Repository e)
            {
                if (e.Url == repository.Url)
                {
                    RetryRepositoryStarsJob.UpdatedRepositorySavedToDatabase -=
                        HandleScheduleRetryRepositoriesStarsCompleted;
                    backgroundStarsTCS.SetResult(e.StarredAt ??
                                                 throw new InvalidOperationException(
                                                     $"{nameof(e.StarredAt)} cannot be null"));
                }
            }
        }
    }

    async Task<(IReadOnlyList<DailyViewsModel> RepositoryViews, IReadOnlyList<DailyClonesModel> RepositoryClones)>
        GetViewsClonesData(Repository repository, CancellationToken cancellationToken)
    {
        if (_backgroundFetchService.IsFetchingViewsClonesStarsInBackground(repository))
        {
            var backgroundServiceResults =
                await getRepositoryViewsClonesStarsFromBackgroundService(repository, cancellationToken)
                    .ConfigureAwait(false);
            return (backgroundServiceResults.RepositoryViews, backgroundServiceResults.RepositoryClones);
        }
        else
        {
            var getRepositoryViewStatisticsTask =
                _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name,
                    cancellationToken);
            var getRepositoryCloneStatisticsTask =
                _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name,
                    cancellationToken);

            await Task.WhenAll(getRepositoryViewStatisticsTask, getRepositoryCloneStatisticsTask).ConfigureAwait(false);

            var repositoryViewsResponse = await getRepositoryViewStatisticsTask.ConfigureAwait(false);
            var repositoryClonesResponse = await getRepositoryCloneStatisticsTask.ConfigureAwait(false);

            return (repositoryViewsResponse.DailyViewsList, repositoryClonesResponse.DailyClonesList);
        }

        async Task<(IReadOnlyList<DateTimeOffset> RepositoryStars, IReadOnlyList<DailyViewsModel> RepositoryViews,
            IReadOnlyList<DailyClonesModel> RepositoryClones)> getRepositoryViewsClonesStarsFromBackgroundService(
            Repository repository, CancellationToken cancellationToken)
        {
            var backgroundStarsTCS =
                new TaskCompletionSource<(IReadOnlyList<DateTimeOffset> RepositoryStars, IReadOnlyList<DailyViewsModel>
                    RepositoryViews, IReadOnlyList<DailyClonesModel> RepositoryClones)>();

            RetryRepositoriesViewsClonesStarsJob.JobCompleted +=
                HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;

            await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            {
                RetryRepositoriesViewsClonesStarsJob.JobCompleted -=
                    HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
                backgroundStarsTCS.SetCanceled();
            }); // Work-around to use a CancellationToken with a TaskCompletionSource: https://stackoverflow.com/a/39897392/5953643

            return await backgroundStarsTCS.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

            void HandleScheduleRetryRepositoriesViewsClonesStarsCompleted(object? sender, Repository e)
            {
                if (e.Url == repository.Url)
                {
                    RetryRepositoriesViewsClonesStarsJob.JobCompleted -=
                        HandleScheduleRetryRepositoriesViewsClonesStarsCompleted;
                    backgroundStarsTCS.SetResult((
                        e.StarredAt ?? throw new InvalidOperationException($"{nameof(e.StarredAt)} cannot be null"),
                        e.DailyViewsList ??
                        throw new InvalidOperationException($"{nameof(e.DailyViewsList)} cannot be null"),
                        e.DailyClonesList ??
                        throw new InvalidOperationException($"{nameof(e.DailyClonesList)} cannot be null")));
                }
            }
        }
    }

    void OnRepositorySavedToDatabase(in Repository repository) =>
        _repositoryEventManager.RaiseEvent(this, repository, nameof(RepositorySavedToDatabase));

    [Conditional("DEBUG")]
    void PrintDays()
    {
        Trace.WriteLine("Clones");
        foreach (var cloneDay in DailyClonesList.Select(static x => x.Day))
            Trace.WriteLine(cloneDay);

        Debug.WriteLine("");

        Debug.WriteLine("Views");
        foreach (var viewDay in DailyViewsList.Select(static x => x.Day))
            Trace.WriteLine(viewDay);
    }

    async void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var repository = (Repository)query[RepositoryQueryString];
        Title = repository.Name;

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await FetchData(repository, cancellationTokenSource.Token);
    }
}