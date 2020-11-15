using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class TrendsViewModel : BaseViewModel
    {
        public const int MinumumChartHeight = 20;

        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly GitHubApiExceptionService _gitHubApiExceptionService;

        bool _isFetchingData = true;
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
                                GitHubApiV3Service gitHubApiV3Service,
                                GitHubGraphQLApiService gitHubGraphQLApiService,
                                GitHubApiExceptionService gitHubApiExceptionService,
                                TrendsChartSettingsService trendsChartSettingsService) : base(analyticsService, mainThread)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _gitHubApiExceptionService = gitHubApiExceptionService;

            IsViewsSeriesVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
            IsUniqueViewsSeriesVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
            IsClonesSeriesVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
            IsUniqueClonesSeriesVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;

            ViewsCardTappedCommand = new Command(() => IsViewsSeriesVisible = !IsViewsSeriesVisible);
            UniqueViewsCardTappedCommand = new Command(() => IsUniqueViewsSeriesVisible = !IsUniqueViewsSeriesVisible);
            ClonesCardTappedCommand = new Command(() => IsClonesSeriesVisible = !IsClonesSeriesVisible);
            UniqueClonesCardTappedCommand = new Command(() => IsUniqueClonesSeriesVisible = !IsUniqueClonesSeriesVisible);

            RefreshState = RefreshState.Uninitialized;

            FetchDataCommand = new AsyncCommand<(Repository Repository, CancellationToken CancellationToken)>(tuple => ExecuteFetchDataCommand(tuple.Repository, tuple.CancellationToken));
        }

        public ICommand ViewsCardTappedCommand { get; }
        public ICommand UniqueViewsCardTappedCommand { get; }
        public ICommand ClonesCardTappedCommand { get; }
        public ICommand UniqueClonesCardTappedCommand { get; }

        public IAsyncCommand<(Repository Repository, CancellationToken CancellationToken)> FetchDataCommand { get; }

        public double DailyViewsClonesMinValue { get; } = 0;
        public double MinDailyStarsValue { get; } = 0;

        public double TotalStars => DailyStarsList.Any() ? DailyStarsList.Last().TotalStars : 0;

        public bool IsStarsEmptyDataViewVisible => !IsStarsChartVisible && !IsFetchingData;
        public bool IsStarsChartVisible => !IsFetchingData && TotalStars > 1;

        public bool IsViewsClonesEmptyDataViewVisible => !IsViewsClonesChartVisible && !IsFetchingData;
        public bool IsViewsClonesChartVisible => !IsFetchingData && DailyViewsList.Sum(x => x.TotalViews + x.TotalUniqueViews) + DailyClonesList.Sum(x => x.TotalClones + x.TotalUniqueClones) > 0;

        public double ViewsClonesChartYAxisInterval => DailyViewsClonesMaxValue > 20 ? Math.Round(DailyViewsClonesMaxValue / 10) : 2;
        public double StarsChartYAxisInterval => MaxDailyStarsValue > 20 ? Math.Round(MaxDailyStarsValue / 10) : 2;

        public DateTime MinViewsClonesDate => DateTimeService.GetMinimumLocalDateTime(DailyViewsList, DailyClonesList);
        public DateTime MaxViewsClonesDate => DateTimeService.GetMaximumLocalDateTime(DailyViewsList, DailyClonesList);

        public DateTime MaxDailyStarsDate => DailyStarsList.Any() ? DailyStarsList.Last().LocalDay : DateTime.Today;
        public DateTime MinDailyStarsDate => DailyStarsList.Any() ? DailyStarsList.First().LocalDay : DateTime.Today.Subtract(TimeSpan.FromDays(14));

        public double MaxDailyStarsValue => TotalStars > MinumumChartHeight ? TotalStars : MinumumChartHeight;

        public double DailyViewsClonesMaxValue
        {
            get
            {
                var dailyViewMaxValue = DailyViewsList.Any() ? DailyViewsList.Max(x => x.TotalViews) : 0;
                var dailyClonesMaxValue = DailyClonesList.Any() ? DailyClonesList.Max(x => x.TotalClones) : 0;

                return Math.Max(Math.Max(dailyViewMaxValue, dailyClonesMaxValue), MinumumChartHeight);
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

        public bool IsFetchingData
        {
            get => _isFetchingData;
            set => SetProperty(ref _isFetchingData, value, OnIsFetchingDataChanged);
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

        RefreshState RefreshState
        {
            set
            {
                ViewsClonesEmptyDataViewImage = EmptyDataViewService.GetViewsClonesImage(value);
                ViewsClonesEmptyDataViewTitleText = EmptyDataViewService.GetViewsClonesTitleText(value);

                StarsEmptyDataViewImage = EmptyDataViewService.GetStarsImage(value, TotalStars);
                StarsEmptyDataViewTitleText = EmptyDataViewService.GetStarsTitleText(value, TotalStars);
                StarsEmptyDataViewDescriptionText = EmptyDataViewService.GetStarsEmptyDataViewDescriptionText(value, TotalStars);

                StarsHeaderMessageText = TotalStars switch
                {
                    0 or 1 => TrendsChartTitleConstants.YouGotThis,
                    _ => TrendsChartTitleConstants.KeepItUp
                };
            }
        }

        async Task ExecuteFetchDataCommand(Repository repository, CancellationToken cancellationToken)
        {
            var refreshState = RefreshState.Uninitialized;

            IReadOnlyList<DateTimeOffset> repositoryStars = Array.Empty<DateTimeOffset>();
            IReadOnlyList<DailyViewsModel> repositoryViews = Array.Empty<DailyViewsModel>();
            IReadOnlyList<DailyClonesModel> repositoryClones = Array.Empty<DailyClonesModel>();

            var minimumTimeTask = Task.Delay(TimeSpan.FromSeconds(1));

            try
            {
                if (repository.DailyClonesList.Any() && repository.DailyViewsList.Any())
                {
                    repositoryStars = repository.StarredAt;
                    repositoryViews = repository.DailyViewsList;
                    repositoryClones = repository.DailyClonesList;
                }
                else
                {
                    IsFetchingData = true;

                    var getStarGazersTask = _gitHubGraphQLApiService.GetStarGazers(repository.Name, repository.OwnerLogin, cancellationToken);
                    var getRepositoryViewStatisticsTask = _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name, cancellationToken);
                    var getRepositoryCloneStatisticsTask = _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name, cancellationToken);

                    await Task.WhenAll(getRepositoryViewStatisticsTask, getRepositoryCloneStatisticsTask, getStarGazersTask).ConfigureAwait(false);

                    var starGazersResponse = await getStarGazersTask.ConfigureAwait(false);
                    var repositoryViewsResponse = await getRepositoryViewStatisticsTask.ConfigureAwait(false);
                    var repositoryClonesResponse = await getRepositoryCloneStatisticsTask.ConfigureAwait(false);

                    repositoryStars = starGazersResponse.StarredAt.Select(x => x.StarredAt).ToList();
                    repositoryViews = repositoryViewsResponse.DailyViewsList;
                    repositoryClones = repositoryClonesResponse.DailyClonesList;
                }

                refreshState = RefreshState.Succeeded;
            }
            catch (Exception e) when (e is ApiException exception && exception.StatusCode is HttpStatusCode.Unauthorized)
            {
                repositoryStars = Array.Empty<DateTimeOffset>();
                repositoryViews = Array.Empty<DailyViewsModel>();
                repositoryClones = Array.Empty<DailyClonesModel>();

                refreshState = RefreshState.LoginExpired;
            }
            catch (Exception e) when (_gitHubApiExceptionService.HasReachedMaximimApiCallLimit(e))
            {
                var responseHeaders = e switch
                {
                    ApiException exception => exception.Headers,
                    GraphQLException graphQLException => graphQLException.ResponseHeaders,
                    _ => throw new NotSupportedException()
                };

                repositoryStars = Array.Empty<DateTimeOffset>();
                repositoryViews = Array.Empty<DailyViewsModel>();
                repositoryClones = Array.Empty<DailyClonesModel>();

                refreshState = RefreshState.MaximumApiLimit;
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e);

                repositoryStars = Array.Empty<DateTimeOffset>();
                repositoryViews = Array.Empty<DailyViewsModel>();
                repositoryClones = Array.Empty<DailyClonesModel>();

                refreshState = RefreshState.Error;
            }
            finally
            {
                DailyStarsList = GetDailyStarsList(repositoryStars).OrderBy(x => x.Day).ToList();
                DailyViewsList = repositoryViews.OrderBy(x => x.Day).ToList();
                DailyClonesList = repositoryClones.OrderBy(x => x.Day).ToList();

                StarsStatisticsText = repositoryStars.Count.ToAbbreviatedText();

                ViewsStatisticsText = repositoryViews.Sum(x => x.TotalViews).ToAbbreviatedText();
                UniqueViewsStatisticsText = repositoryViews.Sum(x => x.TotalUniqueViews).ToAbbreviatedText();

                ClonesStatisticsText = repositoryClones.Sum(x => x.TotalClones).ToAbbreviatedText();
                UniqueClonesStatisticsText = repositoryClones.Sum(x => x.TotalUniqueClones).ToAbbreviatedText();

                //Set RefreshState last, because EmptyDataViews are dependent on the Chart ItemSources, e.g. DailyStarsList
                RefreshState = refreshState;

                //Display the Activity Indicator for a minimum time to ensure consistant UX
                await minimumTimeTask.ConfigureAwait(false);
                IsFetchingData = false;
            }

            PrintDays();
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

        void OnIsFetchingDataChanged()
        {
            OnPropertyChanged(nameof(IsStarsChartVisible));
            OnPropertyChanged(nameof(IsStarsEmptyDataViewVisible));

            OnPropertyChanged(nameof(IsViewsClonesChartVisible));
            OnPropertyChanged(nameof(IsViewsClonesEmptyDataViewVisible));
        }

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
