﻿namespace GitTrends.Common;

public interface IDailyViewsModel
{
	public DateTime LocalDay { get; }

	public DateTimeOffset Day { get; }

	public long TotalViews { get; }

	public long TotalUniqueViews { get; }
}