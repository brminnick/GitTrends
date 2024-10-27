namespace GitTrends;

public record DailyStarsModel(double TotalStars, DateTimeOffset Day)
{
	public DateTime LocalDay => Day.LocalDateTime;
}