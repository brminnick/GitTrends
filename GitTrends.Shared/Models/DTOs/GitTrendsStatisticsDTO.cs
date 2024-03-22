using System;
using System.Collections.Generic;

namespace GitTrends.Shared;

public record GitTrendsStatisticsDTO(Uri GitHubUri, long Stars, long Watchers, long Forks, IReadOnlyList<Contributor> Contributors);