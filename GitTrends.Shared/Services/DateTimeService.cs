using System;
using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
    public static class DateTimeService
    {
        public static DateTimeOffset GetMinimumDateTimeOffset(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList)
        {
            var minViewsDateTimeOffset = dailyViewsList?.Any() is true ? dailyViewsList.Min(x => x.Day) : DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
            var minClonesDateTimeOffset = dailyClonesList?.Any() is true ? dailyClonesList.Min(x => x.Day) : DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

            return new DateTime(Math.Min(minViewsDateTimeOffset.Ticks, minClonesDateTimeOffset.Ticks));
        }

        public static DateTimeOffset GetMaximumDateTimeOffset(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList)
        {
            var maxViewsDateTime = dailyViewsList?.Any() is true ? dailyViewsList.Max(x => x.Day) : DateTimeOffset.UtcNow;
            var maxClonesDateTime = dailyClonesList?.Any() is true ? dailyClonesList.Max(x => x.Day) : DateTimeOffset.UtcNow;

            return new DateTime(Math.Max(maxViewsDateTime.Ticks, maxClonesDateTime.Ticks));
        }

        public static DateTime GetMinimumLocalDateTime(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList)
        {
            var minViewsDateTimeOffset = dailyViewsList?.Any() is true ? dailyViewsList.Min(x => x.LocalDay) : DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
            var minClonesDateTimeOffset = dailyClonesList?.Any() is true ? dailyClonesList.Min(x => x.LocalDay) : DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

            return new DateTime(Math.Min(minViewsDateTimeOffset.Ticks, minClonesDateTimeOffset.Ticks));
        }

        public static DateTime GetMaximumLocalDateTime(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList)
        {
            var maxViewsDateTime = dailyViewsList?.Any() is true ? dailyViewsList.Max(x => x.LocalDay) : DateTimeOffset.UtcNow;
            var maxClonesDateTime = dailyClonesList?.Any() is true  ? dailyClonesList.Max(x => x.LocalDay) : DateTimeOffset.UtcNow;

            return new DateTime(Math.Max(maxViewsDateTime.Ticks, maxClonesDateTime.Ticks));
        }
    }
}
