using System;
namespace GitTrends
{
    public class DailyStarsModel
    {
        public DailyStarsModel(double totalStars, DateTimeOffset day) =>
            (TotalStars, Day) = (totalStars, day);

        public DateTime LocalDay => Day.LocalDateTime;

        public double TotalStars { get; }
        public DateTimeOffset Day { get; }
    }
}
