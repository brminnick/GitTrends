namespace  GitTrends.Common;

public interface IBaseDailyModel
{
	DateTime LocalDay { get; }
	DateTimeOffset Day { get; }
	long TotalCount { get; }
	long TotalUniqueCount { get; }
}