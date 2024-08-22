using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GitTrends.Shared;

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
	public static Repository Deserialize(string json)
	{
		var repositoryJsonObject = JsonNode.Parse(json) ?? throw new InvalidOperationException("Failed to parse string to JsonNode");
		
		return new Repository(
			repositoryJsonObject[nameof(Repository.Name)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.Name)}"),
			repositoryJsonObject[nameof(Repository.Description)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.Description)}"),
			repositoryJsonObject[nameof(Repository.ForkCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.ForkCount)}"),
			repositoryJsonObject[nameof(Repository.OwnerLogin)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.OwnerLogin)}"),
			repositoryJsonObject[nameof(Repository.OwnerAvatarUrl)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.OwnerAvatarUrl)}"),
			repositoryJsonObject[nameof(Repository.IssuesCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.IssuesCount)}"),
			repositoryJsonObject[nameof(Repository.WatchersCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.WatchersCount)}"),
			repositoryJsonObject[nameof(Repository.StarCount)]?.GetValue<long>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.StarCount)}"),
			repositoryJsonObject[nameof(Repository.Url)]?.GetValue<string>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.Url)}"),
			repositoryJsonObject[nameof(Repository.IsFork)]?.GetValue<bool>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.IsFork)}"),
			repositoryJsonObject[nameof(Repository.DataDownloadedAt)]?.GetValue<DateTimeOffset>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.DataDownloadedAt)}"),
			(RepositoryPermission)(repositoryJsonObject[nameof(Repository.Permission)]?.GetValue<int>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.Permission)}")),
			repositoryJsonObject[nameof(Repository.IsArchived)]?.GetValue<bool>() ?? throw new InvalidOperationException($"Error deserializing {nameof(Repository.IsArchived)}"),
			repositoryJsonObject[nameof(Repository.IsFavorite)]?.GetValue<bool?>(),
			repositoryJsonObject[nameof(Repository.DailyViewsList)]?.Deserialize<IEnumerable<DailyViewsModel>>(),
			repositoryJsonObject[nameof(Repository.DailyClonesList)]?.Deserialize<IEnumerable<DailyClonesModel>>(),
			repositoryJsonObject[nameof(Repository.StarredAt)]?.Deserialize<IEnumerable<DateTimeOffset>>());
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