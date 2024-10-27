using System;
using System.Collections.Generic;
using System.Linq;
using GitTrends.Common;

namespace GitTrends.UITests
{
	public class Repository : IRepository
	{
		public DateTimeOffset DataDownloadedAt { get; init; }

		public string OwnerLogin { get; init; } = string.Empty;

		public long StarCount { get; init; }

		public long IssuesCount { get; init; }

		public string Name { get; init; } = string.Empty;

		public string Description { get; init; } = string.Empty;

		public long ForkCount { get; init; }

		public long WatchersCount { get; init; }

		public string OwnerAvatarUrl { get; init; } = string.Empty;

		public string Url { get; init; } = string.Empty;

		public bool IsArchived { get; init; }

		public RepositoryPermission Permission { get; init; }

		public long? TotalViews { get; init; }

		public long? TotalUniqueViews { get; init; }

		public long? TotalClones { get; init; }

		public long? TotalUniqueClones { get; init; }

		public bool IsTrending { get; init; }

		public bool? IsFavorite { get; init; }

		public IReadOnlyList<DailyViewsModel>? DailyViewsList { get; init; } = [];

		public IReadOnlyList<DailyClonesModel>? DailyClonesList { get; init; } = [];
	}
}