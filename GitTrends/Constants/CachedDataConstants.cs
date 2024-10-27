using System;

namespace GitTrends;

public static class CachedDataConstants
{
	public static TimeSpan StarsDataCacheLifeSpan { get; } = TimeSpan.FromHours(12);
	public static TimeSpan ViewsClonesCacheLifeSpan { get; } = TimeSpan.FromHours(1);
	public static TimeSpan DatbaseRepositoryLifeSpan { get; } = TimeSpan.FromDays(14);
	public static TimeSpan DatabaseReferringSitesLifeSpan { get; } = TimeSpan.FromDays(30);
}