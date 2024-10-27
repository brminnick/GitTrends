using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace  GitTrends.Common;

public record Repository : IRepository
{
	readonly IReadOnlyList<DateTimeOffset>? _starredAt;
	readonly IReadOnlyList<DailyViewsModel>? _dailyViewsList;
	readonly IReadOnlyList<DailyClonesModel>? _dailyClonesList;

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
		bool isArchived,
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
		IsArchived = isArchived;
		StarredAt = starredAt?.ToList();
		DailyViewsList = views?.ToList();
		DailyClonesList = clones?.ToList();
	}

	public bool ContainsStarsData => StarredAt is not null;
	public bool ContainsViewsClonesData => TotalClones is not null && TotalViews is not null && TotalUniqueClones is not null && TotalUniqueViews is not null;
	public bool ContainsViewsClonesStarsData => ContainsViewsClonesData && ContainsStarsData;

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
	public bool IsArchived { get; init; }

	public bool? IsFavorite { get; init; }

	public IReadOnlyList<DailyViewsModel>? DailyViewsList
	{
		get => _dailyViewsList;
		init
		{
			var dailyViewsList = value is null ? null : AddMissingDates(value);
			_dailyViewsList = dailyViewsList;

			TotalViews = dailyViewsList?.Sum(static x => x.TotalViews);
			TotalUniqueViews = dailyViewsList?.Sum(static x => x.TotalUniqueViews);

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

			TotalClones = dailyClonesList?.Sum(static x => x.TotalClones);
			TotalUniqueClones = dailyClonesList?.Sum(static x => x.TotalUniqueClones);

			IsTrending |= dailyClonesList?.IsTrending() ?? false;
		}
	}

	public IReadOnlyList<DateTimeOffset>? StarredAt
	{
		get => _starredAt;
		init => _starredAt = value?.OrderBy(static x => x).ToList();
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

	// Manually deserialize Repository to avoid affecting HttpResponse from GitHub API
	public static bool TryParse(string json, [NotNullWhen(true)] out Repository? repository)
	{
		try
		{
			var repositoryJsonObject = JsonNode.Parse(json) ?? throw new InvalidOperationException("Failed to parse string to JsonNode");

			repository = new Repository(
				repositoryJsonObject[nameof(Name)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Name)}"),
				repositoryJsonObject[nameof(Description)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Description)}"),
				repositoryJsonObject[nameof(ForkCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(ForkCount)}"),
				repositoryJsonObject[nameof(OwnerLogin)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(OwnerLogin)}"),
				repositoryJsonObject[nameof(OwnerAvatarUrl)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(OwnerAvatarUrl)}"),
				repositoryJsonObject[nameof(IssuesCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(IssuesCount)}"),
				repositoryJsonObject[nameof(WatchersCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(WatchersCount)}"),
				repositoryJsonObject[nameof(StarCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(StarCount)}"),
				repositoryJsonObject[nameof(Url)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Url)}"),
				repositoryJsonObject[nameof(IsFork)]?.GetValue<bool>() ?? throw new InvalidOperationException($"Error deserializing {nameof(IsFork)}"),
				repositoryJsonObject[nameof(DataDownloadedAt)]?.GetValue<DateTimeOffset>() ?? throw new InvalidOperationException($"Error deserializing {nameof(DataDownloadedAt)}"),
				(RepositoryPermission)(repositoryJsonObject[nameof(Permission)]?.GetValue<int>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Permission)}")),
				repositoryJsonObject[nameof(IsArchived)]?.GetValue<bool>() ?? throw new InvalidOperationException($"Error deserializing {nameof(IsArchived)}"),
				repositoryJsonObject[nameof(IsFavorite)]?.GetValue<bool?>(),
				repositoryJsonObject[nameof(DailyViewsList)]?.Deserialize<IEnumerable<DailyViewsModel>>(),
				repositoryJsonObject[nameof(DailyClonesList)]?.Deserialize<IEnumerable<DailyClonesModel>>(),
				repositoryJsonObject[nameof(StarredAt)]?.Deserialize<IEnumerable<DateTimeOffset>>());

			return true;
		}
		catch
		{
			repository = null;
			return false;
		}
	}

	static IReadOnlyList<DailyViewsModel> AddMissingDates(in IEnumerable<DailyViewsModel> dailyViews)
	{
		var dailyViewsList = new List<DailyViewsModel>(dailyViews);

		var day = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
		var maximumAllowedDay = DateTimeOffset.UtcNow;

		var daysList = dailyViews.Select(static x => x.Day.Day).ToList();

		while (day.Day != maximumAllowedDay.AddDays(1).Day)
		{
			if (!daysList.Contains(day.Day))
				dailyViewsList.Add(new DailyViewsModel(day.RemoveHourMinuteSecond(), 0, 0));

			day = day.AddDays(1);
		}

		return dailyViewsList.Count > 14
			? dailyViewsList.Skip(dailyViewsList.Count - 14).ToList()
			: dailyViewsList;
	}

	static IReadOnlyList<DailyClonesModel> AddMissingDates(in IEnumerable<DailyClonesModel> dailyClones)
	{
		var dailyClonesList = new List<DailyClonesModel>(dailyClones);

		var day = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));
		var maximumAllowedDay = DateTimeOffset.UtcNow;

		var daysList = dailyClones.Select(static x => x.Day.Day).ToList();

		while (day.Day != maximumAllowedDay.AddDays(1).Day)
		{
			if (!daysList.Contains(day.Day))
				dailyClonesList.Add(new DailyClonesModel(day.RemoveHourMinuteSecond(), 0, 0));

			day = day.AddDays(1);
		}

		return dailyClonesList.Count > 14
			? dailyClonesList.Skip(dailyClonesList.Count - 14).ToList()
			: dailyClonesList;
	}
}