using System;
using Newtonsoft.Json;

namespace GitTrends.Shared;

public abstract record BaseDailyModel
{
	protected BaseDailyModel(DateTimeOffset day, long totalViews, long totalUniqueViews) =>
		(Day, TotalCount, TotalUniqueCount) = (day, totalViews, totalUniqueViews);

	[JsonIgnore]
	public DateTime LocalDay => Day.LocalDateTime;

	public DateTimeOffset Day { get; }
	protected long TotalCount { get; }
	protected long TotalUniqueCount { get; }
}