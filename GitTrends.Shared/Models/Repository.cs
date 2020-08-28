using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitTrends.Shared
{
    public class Repository : IRepository
    {
        public Repository(string name, string description, long forkCount, string ownerLogin, string ownerAvatarUrl, long issuesCount, string url, bool isFork, DateTimeOffset dataDownloadedAt, bool? isFavorite = null, IList<DailyViewsModel>? views = null, IList<DailyClonesModel>? clones = null, IEnumerable<DateTimeOffset>? starredAt = null)
        {
            IsFavorite = isFavorite;
            DataDownloadedAt = dataDownloadedAt;

            StarredAt = starredAt?.OrderBy(x => x).ToList() ?? new List<DateTimeOffset>();
            StarCount = StarredAt.Count;

            Name = name;
            Description = description;
            ForkCount = forkCount;
            OwnerLogin = ownerLogin;
            OwnerAvatarUrl = ownerAvatarUrl;
            IssuesCount = issuesCount;
            Url = url;
            IsFork = isFork;

            if (views != null && clones != null)
                AddMissingDates(views, clones);

            DailyViewsList = (views ?? Enumerable.Empty<DailyViewsModel>()).ToList();
            DailyClonesList = (clones ?? Enumerable.Empty<DailyClonesModel>()).ToList();

            TotalViews = DailyViewsList.Sum(x => x.TotalViews);
            TotalUniqueViews = DailyViewsList.Sum(x => x.TotalUniqueViews);
            TotalClones = DailyClonesList.Sum(x => x.TotalClones);
            TotalUniqueClones = DailyClonesList.Sum(x => x.TotalUniqueClones);

            var (isViewsTrending, isClonesTrending) = TrendingService.IsTrending(this);
            IsTrending = (isViewsTrending ?? false) || (isClonesTrending ?? false);
        }

        public DateTimeOffset DataDownloadedAt { get; }

        public long TotalViews { get; }
        public long TotalUniqueViews { get; }
        public long TotalClones { get; }
        public long TotalUniqueClones { get; }

        public string OwnerLogin { get; }
        public string OwnerAvatarUrl { get; }
        public long StarCount { get; }
        public long IssuesCount { get; }
        public string Name { get; }
        public string Description { get; }
        public long ForkCount { get; }
        public bool IsFork { get; }
        public string Url { get; }

        public bool? IsFavorite { get; }
        public bool IsTrending { get; }

        public IReadOnlyList<DailyViewsModel> DailyViewsList { get; }
        public IReadOnlyList<DailyClonesModel> DailyClonesList { get; }
        public IReadOnlyList<DateTimeOffset> StarredAt { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
            stringBuilder.AppendLine($"{nameof(OwnerLogin)}: {OwnerLogin}");
            stringBuilder.AppendLine($"{nameof(OwnerAvatarUrl)}: {OwnerAvatarUrl}");
            stringBuilder.AppendLine($"{nameof(StarCount)}: {StarCount}");
            stringBuilder.AppendLine($"{nameof(Description)}: {Description}");
            stringBuilder.AppendLine($"{nameof(ForkCount)}: {ForkCount}");
            stringBuilder.AppendLine($"{nameof(IssuesCount)}: {IssuesCount}");
            stringBuilder.AppendLine($"{nameof(DataDownloadedAt)}: {DataDownloadedAt}");

            return stringBuilder.ToString();
        }

        static void AddMissingDates(in IList<DailyViewsModel> dailyViewsList, in IList<DailyClonesModel> dailyClonesList)
        {
            var day = DateTimeService.GetMinimumDateTimeOffset(dailyViewsList, dailyClonesList);
            var maximumDay = DateTimeService.GetMaximumDateTimeOffset(dailyViewsList, dailyClonesList);

            var viewsDays = dailyViewsList.Select(x => x.Day.Day).ToList();
            var clonesDays = dailyClonesList.Select(x => x.Day.Day).ToList();

            while (day.Day != maximumDay.AddDays(1).Day)
            {
                if (!viewsDays.Contains(day.Day))
                    dailyViewsList.Add(new DailyViewsModel(removeHourMinuteSecond(day), 0, 0));

                if (!clonesDays.Contains(day.Day))
                    dailyClonesList.Add(new DailyClonesModel(removeHourMinuteSecond(day), 0, 0));

                day = day.AddDays(1);
            }

            static DateTimeOffset removeHourMinuteSecond(in DateTimeOffset date) => new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
        }
    }
}
