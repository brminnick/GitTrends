using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;

namespace GitTrends
{
    class TrendsViewModel : BaseViewModel
    {
        #region Fields
        bool _isFetchingData;
        List<DailyViewsModel> _dailyViewsList;
        List<DailyClonesModel> _dailyClonesList;
        #endregion

        #region Constructors
        public TrendsViewModel()
        {
            FetchDataCommand = new AsyncCommand<(string Owner, string Repository)>(repoTuple => ExecuteFetchDataCommand(repoTuple.Owner, repoTuple.Repository), continueOnCapturedContext: false);
        }
        #endregion

        #region Properties
        public ICommand FetchDataCommand { get; }

        public bool IsChartVisible => !IsFetchingData;
        public double DailyClonesMinValue => DailyClonesList?.Min(x => x.TotalUniqueClones) ?? 0;
        public double DailyClonesMaxValue => DailyClonesList?.Max(x => x.TotalClones) ?? 0;
        public double DailyViewsMinValue => DailyViewsList?.Min(x => x.TotalUniqueViews) ?? 0;
        public double DailyViewsMaxValue => DailyViewsList?.Max(x => x.TotalViews) ?? 0;
        public DateTime MinDateValue => GetMinimumLocalDateTime();
        public DateTime MaxDateValue => GetMaximumLocalDateTime();

        public bool IsFetchingData
        {
            get => _isFetchingData;
            set => SetProperty(ref _isFetchingData, value, () => OnPropertyChanged(nameof(IsChartVisible)));
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
        #endregion

        #region Methods
        async Task ExecuteFetchDataCommand(string owner, string repository)
        {
            IsFetchingData = true;

            try
            {
                var getRepositoryViewStatisticsTask = GitHubApiV3Service.GetRepositoryViewStatistics(owner, repository);
                var getRepositoryCloneStatisticsTask = GitHubApiV3Service.GetRepositoryCloneStatistics(owner, repository);
                var minimumTimeTask = Task.Delay(2000);

                await Task.WhenAll(getRepositoryViewStatisticsTask, getRepositoryCloneStatisticsTask, minimumTimeTask).ConfigureAwait(false);

                var repositoryViewsModel = await getRepositoryViewStatisticsTask.ConfigureAwait(false);
                DailyViewsList = repositoryViewsModel.DailyViewsList;

                var repositoryClonesModel = await getRepositoryCloneStatisticsTask.ConfigureAwait(false);
                DailyClonesList = repositoryClonesModel.DailyClonesList;
            }
            finally
            {
                IsFetchingData = false;
            }
        }

        void UpdateDailyClonesListPropertiesChanged()
        {
            OnPropertyChanged(nameof(DailyClonesMaxValue));
            OnPropertyChanged(nameof(DailyClonesMinValue));
            OnPropertyChanged(nameof(MinDateValue));
            OnPropertyChanged(nameof(MaxDateValue));
        }

        void UpdateDailyViewsListPropertiesChanged()
        {
            OnPropertyChanged(nameof(DailyViewsMaxValue));
            OnPropertyChanged(nameof(DailyViewsMinValue));
            OnPropertyChanged(nameof(MinDateValue));
            OnPropertyChanged(nameof(MaxDateValue));
        }

        DateTime GetMinimumLocalDateTime()
        {
            var minViewsDateTimeOffset = DailyViewsList?.Min(x => x?.LocalDay) ?? DateTime.Today.Subtract(TimeSpan.FromDays(13));
            var minClonesDateTimeOffset = DailyClonesList?.Min(x => x?.LocalDay) ?? DateTime.Today.Subtract(TimeSpan.FromDays(13));

            return new DateTime(Math.Min(minViewsDateTimeOffset.Ticks, minClonesDateTimeOffset.Ticks));
        }

        DateTime GetMaximumLocalDateTime()
        {
            var maxViewsDateTime = DailyViewsList?.Max(x => x?.LocalDay) ?? DateTime.Today;
            var maxClonesDateTime = DailyClonesList?.Max(x => x?.LocalDay) ?? DateTime.Today;

            return new DateTime(Math.Max(maxViewsDateTime.Ticks, maxClonesDateTime.Ticks));
        }
        #endregion
    }
}
