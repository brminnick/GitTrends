using System;
namespace GitTrends.Shared
{
    interface IDailyViewsModel
    {
        public DateTime LocalDay => Day.LocalDateTime;

        public DateTimeOffset Day { get; }

        public long TotalViews { get; }

        public long TotalUniqueViews { get; }
    }
}
