namespace  GitTrends.Common;

public record GitTrendsStatisticsDTO(Uri GitHubUri, long Stars, long Watchers, long Forks, IReadOnlyList<Contributor> Contributors);