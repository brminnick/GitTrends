using System;
namespace GitTrends
{
    public class DailyStarsModel
    {
        public DailyStarsModel(int totalStars, DateTimeOffset date) =>
            (TotalStars, Day) = (totalStars, date);

        public int TotalStars { get; }
        public DateTimeOffset Day { get; }
    }
}
