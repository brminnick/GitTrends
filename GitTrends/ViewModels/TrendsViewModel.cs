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
        #region Fields
        bool _isFetchingData = true;
        List<DailyViewsModel> _dailyViewsList;
        List<DailyClonesModel> _dailyClonesList;
        #endregion

        #region Constructors
        public TrendsViewModel()
        {
            FetchDataCommand = new AsyncCommand<(string Owner, string Repository)>(repoTuple => ExecuteFetchDataCommand(repoTuple.Owner, repoTuple.Repository));
        }
        #endregion

        #region Properties
        public ICommand FetchDataCommand { get; }

        public bool IsChartVisible => !IsFetchingData;
        public double DailyClonesMinValue => 0;
        public double DailyClonesMaxValue => DailyClonesList?.Max(x => x.TotalClones) ?? 0;
        public double DailyViewsMinValue => 0;
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

            await Task.Yield();

            try
            {
                var getRepositoryViewStatisticsTask = GitHubApiV3Service.GetRepositoryViewStatistics(owner, repository);
                var getRepositoryCloneStatisticsTask = GitHubApiV3Service.GetRepositoryCloneStatistics(owner, repository);
                var minimumTimeTask = Task.Delay(2000);

                await Task.WhenAll(getRepositoryViewStatisticsTask, getRepositoryCloneStatisticsTask, minimumTimeTask).ConfigureAwait(false);

                var repositoryViewsModel = await getRepositoryViewStatisticsTask.ConfigureAwait(false);
                var repositoryClonesModel = await getRepositoryCloneStatisticsTask.ConfigureAwait(false);

                addMissingDates(repositoryViewsModel.DailyViewsList, repositoryClonesModel.DailyClonesList);

                DailyViewsList = repositoryViewsModel.DailyViewsList.OrderBy(x => x.Day).ToList();
                DailyClonesList = repositoryClonesModel.DailyClonesList.OrderBy(x => x.Day).ToList();

                PrintDays();
            }
            finally
            {
                IsFetchingData = false;
            }

            void addMissingDates(in List<DailyViewsModel> dailyViewsList, in List<DailyClonesModel> dailyClonesList)
            {
                var day = GetMinimumDateTimeOffset(dailyViewsList, dailyClonesList);
                var maximumDay = GetMaximumDateTimeOffset(dailyViewsList, dailyClonesList);
                while (day.Day != maximumDay.Day + 1)
                {
                    var viewsDays = dailyViewsList.Select(x => x.Day.Day).ToList();
                    if (!viewsDays.Contains(day.Day))
                    {
                        dailyViewsList.Add(new DailyViewsModel(removeHourMinuteSecond(day), 0, 0));
                    }

                    var clonesDays = dailyClonesList.Select(x => x.Day.Day).ToList();
                    if (!clonesDays.Contains(day.Day))
                    {
                        dailyClonesList.Add(new DailyClonesModel(removeHourMinuteSecond(day), 0, 0));
                    }

                    day = day.AddDays(1);
                }
            }

            DateTimeOffset removeHourMinuteSecond(in DateTimeOffset date) => new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
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

        DateTimeOffset GetMinimumDateTimeOffset() => GetMinimumDateTimeOffset(DailyViewsList, DailyClonesList);
        DateTimeOffset GetMaximumDateTimeOffset() => GetMaximumDateTimeOffset(DailyViewsList, DailyClonesList);

        DateTimeOffset GetMinimumDateTimeOffset(in IEnumerable<DailyViewsModel> dailyViewsList, in IEnumerable<DailyClonesModel> dailyClonesList)
        {
            var minViewsDateTimeOffset = dailyViewsList?.Min(x => x?.Day) ?? DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
            var minClonesDateTimeOffset = dailyClonesList?.Min(x => x?.Day) ?? DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

            return new DateTime(Math.Min(minViewsDateTimeOffset.Ticks, minClonesDateTimeOffset.Ticks));
        }

        DateTimeOffset GetMaximumDateTimeOffset(in IEnumerable<DailyViewsModel> dailyViewsList, in IEnumerable<DailyClonesModel> dailyClonesList)
        {
            var maxViewsDateTime = dailyViewsList?.Max(x => x?.Day) ?? DateTimeOffset.UtcNow;
            var maxClonesDateTime = dailyClonesList?.Max(x => x?.Day) ?? DateTimeOffset.UtcNow;

            return new DateTime(Math.Max(maxViewsDateTime.Ticks, maxClonesDateTime.Ticks));
        }

        DateTime GetMinimumLocalDateTime() => GetMinimumLocalDateTime(DailyViewsList, DailyClonesList);
        DateTime GetMaximumLocalDateTime() => GetMaximumLocalDateTime(DailyViewsList, DailyClonesList);

        DateTime GetMinimumLocalDateTime(in IEnumerable<DailyViewsModel> dailyViewsList, in IEnumerable<DailyClonesModel> dailyClonesList)
        {
            var minViewsDateTimeOffset = dailyViewsList?.Min(x => x?.LocalDay) ?? DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
            var minClonesDateTimeOffset = dailyClonesList?.Min(x => x?.LocalDay) ?? DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

            return new DateTime(Math.Min(minViewsDateTimeOffset.Ticks, minClonesDateTimeOffset.Ticks));
        }

        DateTime GetMaximumLocalDateTime(in IEnumerable<DailyViewsModel> dailyViewsList, in IEnumerable<DailyClonesModel> dailyClonesList)
        {
            var maxViewsDateTime = dailyViewsList?.Max(x => x?.LocalDay) ?? DateTimeOffset.UtcNow;
            var maxClonesDateTime = dailyClonesList?.Max(x => x?.LocalDay) ?? DateTimeOffset.UtcNow;

            return new DateTime(Math.Max(maxViewsDateTime.Ticks, maxClonesDateTime.Ticks));
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
        #endregion
    }
}
