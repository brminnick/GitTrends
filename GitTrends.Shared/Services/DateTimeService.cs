using System;
using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
    public static class DateTimeService
    {
        public static DateTimeOffset GetMinimumDateTimeOffset<T>(in IEnumerable<T>? dailyList) where T : BaseDailyModel =>
            dailyList?.Any() is true ? dailyList.Min(x => x.Day) : DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

        public static DateTimeOffset GetMaximumDateTimeOffset<T>(in IEnumerable<T>? dailyList) where T : BaseDailyModel =>
            dailyList?.Any() is true ? dailyList.Max(x => x.Day) : DateTimeOffset.UtcNow;

        public static DateTimeOffset GetMinimumDateTimeOffset(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList)
        {
            var minViewsDateTimeOffset = GetMinimumDateTimeOffset(dailyViewsList);
            var minClonesDateTimeOffset = GetMinimumDateTimeOffset(dailyClonesList);

            return new DateTime(Math.Min(minViewsDateTimeOffset.Ticks, minClonesDateTimeOffset.Ticks));
        }

        public static DateTimeOffset GetMaximumDateTimeOffset(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList)
        {
            var maxViewsDateTime = GetMaximumDateTimeOffset(dailyViewsList);
            var maxClonesDateTime = GetMaximumDateTimeOffset(dailyClonesList);

            return new DateTime(Math.Max(maxViewsDateTime.Ticks, maxClonesDateTime.Ticks));
        }

        public static DateTime GetMinimumLocalDateTime(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList) =>
            GetMinimumDateTimeOffset(dailyViewsList, dailyClonesList).LocalDateTime;

        public static DateTime GetMaximumLocalDateTime(in IEnumerable<DailyViewsModel>? dailyViewsList, in IEnumerable<DailyClonesModel>? dailyClonesList) =>
            GetMaximumDateTimeOffset(dailyViewsList, dailyClonesList).LocalDateTime;
    }
}
