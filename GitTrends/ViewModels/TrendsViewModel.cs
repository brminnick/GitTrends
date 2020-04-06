using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;

namespace GitTrends
{
    class TrendsViewModel : BaseViewModel
    {
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly ReviewService _reviewService;

        bool _isFetchingData = true;
        List<DailyViewsModel> _dailyViewsList = new List<DailyViewsModel>();
        List<DailyClonesModel> _dailyClonesList = new List<DailyClonesModel>();

        public TrendsViewModel(GitHubApiV3Service gitHubApiV3Service,
                                AnalyticsService analyticsService,
                                ReviewService reviewService) : base(analyticsService)
        {
            _reviewService = reviewService;
            _gitHubApiV3Service = gitHubApiV3Service;
            FetchDataCommand = new AsyncCommand<Repository>(repo => ExecuteFetchDataCommand(repo));
        }

        public ICommand FetchDataCommand { get; }
        public double DailyViewsClonesMinValue { get; } = 0;

        public bool AreStatisticsVisible => !IsFetchingData;
        public DateTime MinDateValue => DateTimeService.GetMinimumLocalDateTime(DailyViewsList, DailyClonesList);
        public DateTime MaxDateValue => DateTimeService.GetMaximumLocalDateTime(DailyViewsList, DailyClonesList);

        public double DailyViewsClonesMaxValue
        {
            get
            {
                const int minimumValue = 20;

                var dailyViewMaxValue = DailyViewsList.Any() ? DailyViewsList.Max(x => x.TotalViews) : 0;
                var dailyClonesMaxValue = DailyClonesList.Any() ? DailyClonesList.Max(x => x.TotalClones) : 0;

                return Math.Max(Math.Max(dailyViewMaxValue, dailyClonesMaxValue), minimumValue);
            }
        }

        public bool IsFetchingData
        {
            get => _isFetchingData;
            set => SetProperty(ref _isFetchingData, value, () => OnPropertyChanged(nameof(AreStatisticsVisible)));
        }

        public List<DailyViewsModel> DailyViewsList
        {
            get => _dailyViewsList;
            set => SetProperty(ref _dailyViewsList, value, UpdateDailyViewsListPropertiesChanged);
        }

        public List<DailyClonesModel> DailyClonesList
        {
            get => _dailyClonesList;
            set => SetProperty(ref _dailyClonesList, value, UpdateDailyClonesListPropertiesChanged);
        }

        async Task ExecuteFetchDataCommand(Repository repository)
        {
            _reviewService.TryRequestReview();

            IReadOnlyList<DailyViewsModel> repositoryViews;
            IReadOnlyList<DailyClonesModel> repositoryClones;

            var minimumTimeTask = Task.Delay(2000);

            if (repository.DailyClonesList.Any() && repository.DailyViewsList.Any())
            {
                repositoryViews = repository.DailyViewsList;
                repositoryClones = repository.DailyClonesList;
            }
            else
            {
                try
                {
                    IsFetchingData = true;

                    var getRepositoryViewStatisticsTask = _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name);
                    var getRepositoryCloneStatisticsTask = _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name);

                    await Task.WhenAll(getRepositoryViewStatisticsTask, getRepositoryCloneStatisticsTask).ConfigureAwait(false);

                    var repositoryViewsResponse = await getRepositoryViewStatisticsTask.ConfigureAwait(false);
                    var repositoryClonesResponse = await getRepositoryCloneStatisticsTask.ConfigureAwait(false);

                    repositoryViews = repositoryViewsResponse.DailyViewsList;
                    repositoryClones = repositoryClonesResponse.DailyClonesList;

                }
                catch (Exception e)
                {
                    //ToDo Add note reporting to the user that the statistics are unavailable due to internet connectivity
                    repositoryViews = Enumerable.Empty<DailyViewsModel>().ToList();
                    repositoryClones = Enumerable.Empty<DailyClonesModel>().ToList();

                    AnalyticsService.Report(e);
                }
            }

            //Display the Activity Indicator for a minimum time to ensure consistant UX 
            await minimumTimeTask.ConfigureAwait(false);
            IsFetchingData = false;

            DailyViewsList = repositoryViews.OrderBy(x => x.Day).ToList();
            DailyClonesList = repositoryClones.OrderBy(x => x.Day).ToList();

            PrintDays();
        }

        void UpdateDailyClonesListPropertiesChanged()
        {
            OnPropertyChanged(nameof(DailyViewsClonesMaxValue));
            OnPropertyChanged(nameof(DailyViewsClonesMinValue));
            OnPropertyChanged(nameof(MinDateValue));
            OnPropertyChanged(nameof(MaxDateValue));
        }

        void UpdateDailyViewsListPropertiesChanged()
        {
            OnPropertyChanged(nameof(DailyViewsClonesMaxValue));
            OnPropertyChanged(nameof(DailyViewsClonesMaxValue));
            OnPropertyChanged(nameof(MinDateValue));
            OnPropertyChanged(nameof(MaxDateValue));
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
