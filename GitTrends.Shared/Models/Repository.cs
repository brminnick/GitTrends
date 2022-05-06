using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitTrends.Shared
{
	public record Repository : IRepository
	{
		IReadOnlyList<DateTimeOffset>? _starredAt;
		IReadOnlyList<DailyViewsModel>? _dailyViewsList;
		IReadOnlyList<DailyClonesModel>? _dailyClonesList;

		public Repository(string name,
							string description,
							long forkCount,
							string ownerLogin,
							string ownerAvatarUrl,
							long issuesCount,
							long watchersCount,
							long starCount,
							string url,
							bool isFork,
							DateTimeOffset dataDownloadedAt,
							RepositoryPermission permission,
							bool? isFavorite = null,
							IEnumerable<DailyViewsModel>? views = null,
							IEnumerable<DailyClonesModel>? clones = null,
							IEnumerable<DateTimeOffset>? starredAt = null)
		{
			IsFavorite = isFavorite;
			DataDownloadedAt = dataDownloadedAt;

			Name = name;
			Description = description;
			WatchersCount = watchersCount;
			ForkCount = forkCount;
			OwnerLogin = ownerLogin;
			OwnerAvatarUrl = ownerAvatarUrl;
			IssuesCount = issuesCount;
			StarCount = starCount;
			IsFork = isFork;
			Url = url;
			Permission = permission;
			StarredAt = starredAt?.ToList();
			DailyViewsList = views?.ToList();
			DailyClonesList = clones?.ToList();
		}

		public bool ContainsViewsClonesStarsData => ContainsViewsClonesData && StarredAt is not null && StarredAt.Count == StarCount;
		public bool ContainsViewsClonesData => TotalClones is not null && TotalViews is not null && TotalUniqueClones is not null && TotalUniqueViews is not null;

		public DateTimeOffset DataDownloadedAt { get; init; }
		public string OwnerLogin { get; init; }
		public string OwnerAvatarUrl { get; init; }
		public long IssuesCount { get; init; }
		public string Name { get; init; }
		public string Description { get; init; }
		public long WatchersCount { get; init; }
		public long ForkCount { get; init; }
		public long StarCount { get; init; }
		public bool IsFork { get; init; }
		public string Url { get; init; }
		public RepositoryPermission Permission { get; init; }

		public bool? IsFavorite { get; init; }

		public IReadOnlyList<DailyViewsModel>? DailyViewsList
		{
			get => _dailyViewsList;
			init
			{
				var dailyViewsList = value is null ? null : AddMissingDates(value);
				_dailyViewsList = dailyViewsList;

				TotalViews = dailyViewsList?.Sum(x => x.TotalViews);
				TotalUniqueViews = dailyViewsList?.Sum(x => x.TotalUniqueViews);

				IsTrending |= dailyViewsList?.IsTrending() ?? false;
			}
		}

		public IReadOnlyList<DailyClonesModel>? DailyClonesList
		{
			get => _dailyClonesList;
			init
			{
				var dailyClonesList = value is null ? null : AddMissingDates(value);
				_dailyClonesList = dailyClonesList;

				TotalClones = dailyClonesList?.Sum(x => x.TotalClones);
				TotalUniqueClones = dailyClonesList?.Sum(x => x.TotalUniqueClones);

				IsTrending |= dailyClonesList?.IsTrending() ?? false;
			}
		}

		public IReadOnlyList<DateTimeOffset>? StarredAt
		{
			get => _starredAt;
			init => _starredAt = value?.OrderBy(x => x).ToList();
		}

		public long? TotalViews { get; private init; }
		public long? TotalUniqueViews { get; private init; }
		public long? TotalClones { get; private init; }
		public long? TotalUniqueClones { get; private init; }
		public bool IsTrending { get; private init; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
			stringBuilder.AppendLine($"{nameof(OwnerLogin)}: {OwnerLogin}");
			stringBuilder.AppendLine($"{nameof(OwnerAvatarUrl)}: {OwnerAvatarUrl}");
			stringBuilder.AppendLine($"{nameof(StarCount)}: {StarCount}");
			stringBuilder.AppendLine($"{nameof(Description)}: {Description}");
			stringBuilder.AppendLine($"{nameof(ForkCount)}: {ForkCount}");
			stringBuilder.AppendLine($"{nameof(IssuesCount)}: {IssuesCount}");
			stringBuilder.AppendLine($"{nameof(DataDownloadedAt)}: {DataDownloadedAt}");

			return stringBuilder.ToString();
		}

		public bool IsOwnerAvatarUrlValid() => Uri.TryCreate(OwnerAvatarUrl, UriKind.Absolute, out _);

		public bool IsRepositoryUrlValid() => Uri.TryCreate(Url, UriKind.Absolute, out _);

		static IReadOnlyList<DailyViewsModel> AddMissingDates(in IEnumerable<DailyViewsModel> dailyViews)
		{
			var dailyViewsList = new List<DailyViewsModel>(dailyViews);

			var day = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
			var maximumDay = DateTimeOffset.UtcNow;

			var daysList = dailyViews.Select(x => x.Day.Day).ToList();

			while (day.Day != maximumDay.AddDays(1).Day)
			{
				if (!daysList.Contains(day.Day))
					dailyViewsList.Add(new DailyViewsModel(RemoveHourMinuteSecond(day), 0, 0));

				day = day.AddDays(1);
			}

			return dailyViewsList;
		}

		static IReadOnlyList<DailyClonesModel> AddMissingDates(in IEnumerable<DailyClonesModel> dailyClones)
		{
			var dailyClonesList = new List<DailyClonesModel>(dailyClones);

			var day = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
			var maximumDay = DateTimeOffset.UtcNow;

			var daysList = dailyClones.Select(x => x.Day.Day).ToList();

			while (day.Day != maximumDay.AddDays(1).Day)
			{
				if (!daysList.Contains(day.Day))
					dailyClonesList.Add(new DailyClonesModel(RemoveHourMinuteSecond(day), 0, 0));

				day = day.AddDays(1);
			}

			return dailyClonesList;
		}


		static DateTimeOffset RemoveHourMinuteSecond(in DateTimeOffset date) => new(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
	}
}