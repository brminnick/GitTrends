namespace  GitTrends.Common;

public static class DateTimeService
{
	public static DateTimeOffset GetMinimumDateTimeOffset<T>(in IEnumerable<T>? dailyList) where T : IBaseDailyModel =>
		dailyList?.Any() is true ? dailyList.Min(static x => x.Day) : DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

	public static DateTimeOffset GetMaximumDateTimeOffset<T>(in IEnumerable<T>? dailyList) where T : IBaseDailyModel =>
		dailyList?.Any() is true ? dailyList.Max(static x => x.Day) : DateTimeOffset.UtcNow;

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

	public static IReadOnlyList<DateTimeOffset> GetEstimatedStarredAtList(in Repository repositoryFromDatabase, in long starCount)
	{
		if (starCount is 0)
			return [];

		var incompleteStarredAtList = new List<DateTimeOffset>(repositoryFromDatabase.StarredAt ?? [DateTimeOffset.MinValue]);
		incompleteStarredAtList.Sort();

		var totalMissingTime = DateTimeOffset.UtcNow.Subtract(incompleteStarredAtList.Last());
		var missingStarCount = starCount - incompleteStarredAtList.Count;
		var nextDataPointDeltaInSeconds = totalMissingTime.TotalSeconds / missingStarCount;

		for (var i = 0; i < missingStarCount; i++)
		{
			incompleteStarredAtList.Add(incompleteStarredAtList.Last().AddSeconds(nextDataPointDeltaInSeconds));
		}

		return incompleteStarredAtList;
	}

	public static DateTimeOffset RemoveHourMinuteSecond(this DateTimeOffset date) => new(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
}