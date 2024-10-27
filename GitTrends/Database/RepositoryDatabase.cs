using GitTrends.Common;
using SQLite;

namespace GitTrends;

public class RepositoryDatabase : BaseDatabase
{
	public RepositoryDatabase(IFileSystem fileSystem, IAnalyticsService analyticsService) : base(fileSystem, analyticsService, CachedDataConstants.DatbaseRepositoryLifeSpan)
	{
		GitHubApiRepositoriesService.RepositoryUriNotFound += HandleRepositoryUriNotFound;
	}

	public override async Task DeleteAllData(CancellationToken token)
	{
		await Task.WhenAll(
			Execute<int, DailyViewsDatabaseModel>(static async databaseConnection => await databaseConnection.DeleteAllAsync<DailyViewsDatabaseModel>(), token),
			Execute<int, DailyClonesDatabaseModel>(static async databaseConnection => await databaseConnection.DeleteAllAsync<DailyClonesDatabaseModel>(), token),
			Execute<int, StarGazerInfoDatabaseModel>(static async databaseConnection => await databaseConnection.DeleteAllAsync<StarGazerInfoDatabaseModel>(), token));

		await Execute<int, RepositoryDatabaseModel>(static async databaseConnection => await databaseConnection.DeleteAllAsync<RepositoryDatabaseModel>(), token);
	}

	public async Task DeleteExpiredData(CancellationToken token)
	{
		var dailyClones = await Execute<List<DailyClonesDatabaseModel>, DailyClonesDatabaseModel>(static dailyClonesDatabaseConnection => dailyClonesDatabaseConnection.Table<DailyClonesDatabaseModel>().ToListAsync(), token).ConfigureAwait(false);
		var dailyViews = await Execute<List<DailyViewsDatabaseModel>, DailyViewsDatabaseModel>(static dailyViewsDatabaseConnection => dailyViewsDatabaseConnection.Table<DailyViewsDatabaseModel>().ToListAsync(), token).ConfigureAwait(false);

		var expiredDailyClones = dailyClones.Where(x => IsExpired(x.DownloadedAt));
		var expiredDailyViews = dailyViews.Where(x => IsExpired(x.DownloadedAt));

		foreach (var expiredDailyClone in expiredDailyClones)
			await Execute<int, DailyClonesDatabaseModel>(dailyClonesDatabaseConnection => dailyClonesDatabaseConnection.DeleteAsync(expiredDailyClone), token).ConfigureAwait(false);

		foreach (var expiredDailyView in expiredDailyViews)
			await Execute<int, DailyViewsDatabaseModel>(dailyViewsDatabaseConnection => dailyViewsDatabaseConnection.DeleteAsync(expiredDailyView), token).ConfigureAwait(false);
	}

	public async Task DeleteRepository(Repository repository, CancellationToken token)
	{
		var dailyViews = await Execute<List<DailyViewsDatabaseModel>, DailyViewsDatabaseModel>(static dailyViewsDatabaseConnection => dailyViewsDatabaseConnection.Table<DailyViewsDatabaseModel>().ToListAsync(), token).ConfigureAwait(false);
		var dailyClones = await Execute<List<DailyClonesDatabaseModel>, DailyClonesDatabaseModel>(static dailyClonesDatabaseConnection => dailyClonesDatabaseConnection.Table<DailyClonesDatabaseModel>().ToListAsync(), token).ConfigureAwait(false);
		var starGazerInfos = await Execute<List<StarGazerInfoDatabaseModel>, StarGazerInfoDatabaseModel>(static starGazerInfoDatabaseConnection => starGazerInfoDatabaseConnection.Table<StarGazerInfoDatabaseModel>().ToListAsync(), token).ConfigureAwait(false);

		foreach (var dailyClone in dailyClones.Where(x => x.RepositoryUrl == repository.Url))
			await Execute<int, DailyClonesDatabaseModel>(dailyClonesDatabaseConnection => dailyClonesDatabaseConnection.DeleteAsync(dailyClone), token).ConfigureAwait(false);

		foreach (var dailyView in dailyViews.Where(x => x.RepositoryUrl == repository.Url))
			await Execute<int, DailyViewsDatabaseModel>(dailyViewsDatabaseConnection => dailyViewsDatabaseConnection.DeleteAsync(dailyView), token).ConfigureAwait(false);

		foreach (var starGazerInfo in starGazerInfos.Where(x => x.RepositoryUrl == repository.Url))
			await Execute<int, StarGazerInfoDatabaseModel>(starGazerInfoDatabaseConnection => starGazerInfoDatabaseConnection.DeleteAsync(starGazerInfo), token).ConfigureAwait(false);

		await Execute<int, RepositoryDatabaseModel>(repositoryDatabaseConnection => repositoryDatabaseConnection.DeleteAsync(RepositoryDatabaseModel.ToRepositoryDatabase(repository)), token).ConfigureAwait(false);
	}

	public async Task SaveRepository(Repository repository, CancellationToken token)
	{
		var repositoryDatabaseModel = RepositoryDatabaseModel.ToRepositoryDatabase(repository);
		await Execute<int, RepositoryDatabaseModel>(databaseConnection => databaseConnection.InsertOrReplaceAsync(repositoryDatabaseModel), token).ConfigureAwait(false);

		await SaveStarGazerInfo(repository, token).ConfigureAwait(false);
		await SaveDailyClones(repository, token).ConfigureAwait(false);
		await SaveDailyViews(repository, token).ConfigureAwait(false);
	}

	public async Task<IReadOnlyList<string>> GetFavoritesUrls(CancellationToken token)
	{
		var favoriteRepositories =
			await Execute<List<RepositoryDatabaseModel>, RepositoryDatabaseModel>(
					static repositoryDatabaseConnection => repositoryDatabaseConnection.Table<RepositoryDatabaseModel>().Where(static x => x.IsFavorite == true).ToListAsync(),
					token)
				.ConfigureAwait(false);

		return [.. favoriteRepositories.Select(static x => x.Url)];
	}

	public async Task<Repository?> GetRepository(string repositoryUrl, CancellationToken token)
	{
		var repositoryDatabaseModel =
			await Execute<RepositoryDatabaseModel?, RepositoryDatabaseModel>(async repositoryDatabaseConnection =>
				{
					var repository = await repositoryDatabaseConnection.Table<RepositoryDatabaseModel>().FirstOrDefaultAsync(x => x.Url == repositoryUrl).ConfigureAwait(false);
					return repository ?? null;
				},
				token).ConfigureAwait(false);

		if (repositoryDatabaseModel is null)
			return null;

		var (dailyClones, dailyViews, starGazers) = await GetClonesViewsStars(repositoryDatabaseModel, token).ConfigureAwait(false);

		return RepositoryDatabaseModel.ToRepository(repositoryDatabaseModel, dailyClones, dailyViews, starGazers);
	}

	public async Task<IReadOnlyList<Repository>> GetRepositories(CancellationToken token)
	{
		var repositoryDatabaseModels = await Execute<List<RepositoryDatabaseModel>, RepositoryDatabaseModel>(static repositoryDatabaseConnection => repositoryDatabaseConnection.Table<RepositoryDatabaseModel>().ToListAsync(), token).ConfigureAwait(false);

		if (repositoryDatabaseModels.Count is 0)
			return [];

		var repositoryList = new List<Repository>();
		foreach (var repositoryDatabaseModel in repositoryDatabaseModels)
		{
			var (dailyClones, dailyViews, starGazers) = await GetClonesViewsStars(repositoryDatabaseModel, token).ConfigureAwait(false);

			var repository = RepositoryDatabaseModel.ToRepository(repositoryDatabaseModel, dailyClones, dailyViews, starGazers);
			repositoryList.Add(repository);
		}

		return repositoryList;
	}

	static bool IsWithin14Days(DateTimeOffset dataDate, DateTimeOffset mostRecentDate) => dataDate.CompareTo(mostRecentDate.Subtract(TimeSpan.FromDays(13)).ToLocalTime()) >= 0;

	async Task<(IReadOnlyList<DailyClonesDatabaseModel>? dailyClones,
		IReadOnlyList<DailyViewsDatabaseModel>? dailyViews,
		IReadOnlyList<StarGazerInfoDatabaseModel>? starGazers)> GetClonesViewsStars(RepositoryDatabaseModel repository, CancellationToken token)
	{
		var getStarGazerInfoModelsTask = Execute<List<StarGazerInfoDatabaseModel>, StarGazerInfoDatabaseModel>(starGazerInfoDatabaseConnection => starGazerInfoDatabaseConnection.Table<StarGazerInfoDatabaseModel>().Where(x => x.RepositoryUrl == repository.Url).ToListAsync(), token);
		var getDailyClonesDatabaseModelsTask = Execute<List<DailyClonesDatabaseModel>, DailyClonesDatabaseModel>(dailyClonesDatabaseConnection => dailyClonesDatabaseConnection.Table<DailyClonesDatabaseModel>().Where(x => x.RepositoryUrl == repository.Url).ToListAsync(), token);
		var getDailyViewsDatabaseModelsTask = Execute<List<DailyViewsDatabaseModel>, DailyViewsDatabaseModel>(dailyViewsDatabaseConnection => dailyViewsDatabaseConnection.Table<DailyViewsDatabaseModel>().Where(x => x.RepositoryUrl == repository.Url).ToListAsync(), token);

		await Task.WhenAll(getDailyClonesDatabaseModelsTask, getDailyViewsDatabaseModelsTask, getStarGazerInfoModelsTask).ConfigureAwait(false);

		var starGazerInfoModels = await getStarGazerInfoModelsTask.ConfigureAwait(false);
		var dailyClonesDatabaseModels = await getDailyClonesDatabaseModelsTask.ConfigureAwait(false);
		var dailyViewsDatabaseModels = await getDailyViewsDatabaseModelsTask.ConfigureAwait(false);

		if (starGazerInfoModels.Count is 0)
			starGazerInfoModels = null;

		if (dailyClonesDatabaseModels.Count is 0)
			dailyClonesDatabaseModels = null;

		if (dailyViewsDatabaseModels.Count is 0)
			dailyViewsDatabaseModels = null;

		var sortedRecentDailyClonesDatabaseModels = dailyClonesDatabaseModels?.OrderByDescending(static x => x.DownloadedAt).ToList();
		var sortedRecentDailyViewsDatabaseModels = dailyViewsDatabaseModels?.OrderByDescending(static x => x.DownloadedAt).ToList();
		var sortedStarGazerInfoModels = starGazerInfoModels?.OrderByDescending(static x => x.StarredAt).ToList();

		var mostRecentCloneDay = sortedRecentDailyClonesDatabaseModels?.Count is > 0 ? sortedRecentDailyClonesDatabaseModels.Max(static x => x.Day) : default;
		var mostRecentViewDay = sortedRecentDailyViewsDatabaseModels?.Count is > 0 ? sortedRecentDailyViewsDatabaseModels.Max(static x => x.Day) : default;

		var mostRecentDate = mostRecentCloneDay.CompareTo(mostRecentViewDay) > 0 ? mostRecentCloneDay : mostRecentViewDay;

		var dailyClones = sortedRecentDailyClonesDatabaseModels?.Where(x => IsWithin14Days(x.Day, mostRecentDate)).GroupBy(static x => x.Day).Select(static x => x.First()).Take(14);
		var dailyViews = sortedRecentDailyViewsDatabaseModels?.Where(x => IsWithin14Days(x.Day, mostRecentDate)).GroupBy(static x => x.Day).Select(static x => x.First()).Take(14);

		return (dailyClones?.ToList(), dailyViews?.ToList(), sortedStarGazerInfoModels);
	}

	async Task SaveStarGazerInfo(Repository repository, CancellationToken token)
	{
		if (repository.StarredAt is null)
			return;

		foreach (var starredAtDate in repository.StarredAt)
		{
			var starGazerInfoDatabaseModel = new StarGazerInfoDatabaseModel
			{
				RepositoryUrl = repository.Url,
				StarredAt = starredAtDate
			};

			await Execute<int, StarGazerInfoDatabaseModel>(starGazerInfoDatabaseConnection => starGazerInfoDatabaseConnection.InsertOrReplaceAsync(starGazerInfoDatabaseModel), token).ConfigureAwait(false);
		}
	}

	async Task SaveDailyClones(Repository repository, CancellationToken token)
	{
		if (repository.DailyClonesList is null)
			return;

		foreach (var dailyClonesModel in repository.DailyClonesList)
		{
			var dailyClonesDatabaseModel = DailyClonesDatabaseModel.ToDailyClonesDatabaseModel(dailyClonesModel, repository);
			await Execute<int, DailyClonesDatabaseModel>(dailyClonesDatabaseConnection => dailyClonesDatabaseConnection.InsertOrReplaceAsync(dailyClonesDatabaseModel), token).ConfigureAwait(false);
		}
	}

	async Task SaveDailyViews(Repository repository, CancellationToken token)
	{
		if (repository.DailyViewsList is null)
			return;

		foreach (var dailyViewsModel in repository.DailyViewsList)
		{
			var dailyViewsDatabaseModel = DailyViewsDatabaseModel.ToDailyViewsDatabaseModel(dailyViewsModel, repository);
			await Execute<int, DailyViewsDatabaseModel>(dailyViewsDatabaseConnection => dailyViewsDatabaseConnection.InsertOrReplaceAsync(dailyViewsDatabaseModel), token).ConfigureAwait(false);
		}
	}

	async void HandleRepositoryUriNotFound(object? sender, Uri e)
	{
		var repositoryFromDatabase = await GetRepository(e.ToString(), CancellationToken.None).ConfigureAwait(false);

		if (repositoryFromDatabase is not null)
			await DeleteRepository(repositoryFromDatabase, CancellationToken.None).ConfigureAwait(false);
	}

	sealed record DailyClonesDatabaseModel : IDailyClonesModel
	{
		public DateTime LocalDay => Day.LocalDateTime;

		//PrimaryKey must be nullable https://github.com/praeclarum/sqlite-net/issues/327
		[PrimaryKey]
		public int? Id { get; init; }

		[Indexed]
		public string RepositoryUrl { get; init; } = string.Empty;

		public DateTimeOffset Day { get; init; }

		public DateTimeOffset DownloadedAt { get; init; } = DateTimeOffset.UtcNow;

		public long TotalClones { get; init; }

		public long TotalUniqueClones { get; init; }

		public static DailyClonesModel ToDailyClonesModel(in DailyClonesDatabaseModel dailyClonesDatabaseModel) =>
			new(dailyClonesDatabaseModel.Day, dailyClonesDatabaseModel.TotalClones, dailyClonesDatabaseModel.TotalUniqueClones);

		public static DailyClonesDatabaseModel ToDailyClonesDatabaseModel(in DailyClonesModel dailyClonesModel, in Repository repository) => new()
		{
			DownloadedAt = repository.DataDownloadedAt,
			RepositoryUrl = repository.Url,
			Day = dailyClonesModel.Day,
			TotalClones = dailyClonesModel.TotalClones,
			TotalUniqueClones = dailyClonesModel.TotalUniqueClones
		};
	}

	record StarGazerInfoDatabaseModel : IStarGazerInfo
	{
		//PrimaryKey must be nullable https://github.com/praeclarum/sqlite-net/issues/327
		[PrimaryKey]
		public int? Id { get; init; }

		[Indexed]
		public string RepositoryUrl { get; init; } = string.Empty;

		public DateTimeOffset StarredAt { get; init; }

		public static StarGazerInfo ToStarGazerInfo(in StarGazerInfoDatabaseModel starGazerInfoDatabaseModel) =>
			new(starGazerInfoDatabaseModel.StarredAt, string.Empty);

		public static StarGazerInfoDatabaseModel ToStarGazerInfoDatabaseModel(in StarGazerInfo starGazerInfo, in Repository repository) => new()
		{
			StarredAt = starGazerInfo.StarredAt,
			RepositoryUrl = repository.Url,
		};
	}

	record DailyViewsDatabaseModel : IDailyViewsModel
	{
		public DateTime LocalDay => Day.LocalDateTime;

		//PrimaryKey must be nullable https://github.com/praeclarum/sqlite-net/issues/327
		[PrimaryKey]
		public int? Id { get; init; }

		[Indexed]
		public string RepositoryUrl { get; init; } = string.Empty;

		public DateTimeOffset Day { get; init; }

		public DateTimeOffset DownloadedAt { get; init; } = DateTimeOffset.UtcNow;

		public long TotalViews { get; init; }

		public long TotalUniqueViews { get; init; }

		public static DailyViewsModel ToDailyViewsModel(in DailyViewsDatabaseModel dailyViewsDatabaseModel) =>
			new(dailyViewsDatabaseModel.Day, dailyViewsDatabaseModel.TotalViews, dailyViewsDatabaseModel.TotalUniqueViews);

		public static DailyViewsDatabaseModel ToDailyViewsDatabaseModel(in DailyViewsModel dailyViewsModel, in Repository repository) => new()
		{
			DownloadedAt = repository.DataDownloadedAt,
			RepositoryUrl = repository.Url,
			Day = dailyViewsModel.Day,
			TotalViews = dailyViewsModel.TotalViews,
			TotalUniqueViews = dailyViewsModel.TotalUniqueViews
		};
	}

	record RepositoryDatabaseModel : IRepository
	{
		public DateTimeOffset DataDownloadedAt { get; init; } = DateTimeOffset.UtcNow;

		public string Name { get; init; } = string.Empty;

		public string Description { get; init; } = string.Empty;

		public long ForkCount { get; init; }

		[PrimaryKey]
		public string Url { get; init; } = string.Empty;

		public long StarCount { get; init; }

		public string OwnerLogin { get; init; } = string.Empty;

		public string OwnerAvatarUrl { get; init; } = string.Empty;

		public long IssuesCount { get; init; }

		public long WatchersCount { get; init; }

		public RepositoryPermission Permission { get; init; }

		public bool IsFork { get; init; }

		public bool IsArchived { get; init; }

		public long? TotalViews { get; init; }

		public long? TotalUniqueViews { get; init; }

		public long? TotalClones { get; init; }

		public long? TotalUniqueClones { get; init; }


		[Indexed]
		public bool? IsFavorite { get; init; }

		public static Repository ToRepository(in RepositoryDatabaseModel repositoryDatabaseModel,
			in IEnumerable<DailyClonesDatabaseModel>? dailyClonesDatabaseModels,
			in IEnumerable<DailyViewsDatabaseModel>? dailyViewsDatabaseModels,
			in IEnumerable<StarGazerInfoDatabaseModel>? starGazerInfoDatabaseModels)
		{
			var clonesList = dailyClonesDatabaseModels?.Select(static x => DailyClonesDatabaseModel.ToDailyClonesModel(x));
			var viewsList = dailyViewsDatabaseModels?.Select(static x => DailyViewsDatabaseModel.ToDailyViewsModel(x));

			return new Repository(repositoryDatabaseModel.Name,
				repositoryDatabaseModel.Description,
				repositoryDatabaseModel.ForkCount,
				repositoryDatabaseModel.OwnerLogin,
				repositoryDatabaseModel.OwnerAvatarUrl ?? repositoryDatabaseModel.OwnerLogin,
				repositoryDatabaseModel.IssuesCount,
				repositoryDatabaseModel.WatchersCount,
				repositoryDatabaseModel.StarCount,
				repositoryDatabaseModel.Url,
				repositoryDatabaseModel.IsFork,
				repositoryDatabaseModel.DataDownloadedAt,
				repositoryDatabaseModel.Permission,
				repositoryDatabaseModel.IsArchived,
				repositoryDatabaseModel.IsFavorite,
				viewsList,
				clonesList,
				starGazerInfoDatabaseModels?.Select(static x => x.StarredAt).Distinct());
		}

		public static RepositoryDatabaseModel ToRepositoryDatabase(in Repository repository) => new()
		{
			DataDownloadedAt = repository.DataDownloadedAt,
			Description = repository.Description,
			StarCount = repository.StarCount,
			Url = repository.Url,
			IssuesCount = repository.IssuesCount,
			ForkCount = repository.ForkCount,
			WatchersCount = repository.WatchersCount,
			Name = repository.Name,
			OwnerAvatarUrl = repository.OwnerAvatarUrl,
			OwnerLogin = repository.OwnerLogin,
			IsFork = repository.IsFork,
			TotalClones = repository.TotalClones,
			TotalUniqueClones = repository.TotalUniqueClones,
			TotalViews = repository.TotalViews,
			TotalUniqueViews = repository.TotalUniqueViews,
			IsFavorite = repository.IsFavorite
		};
	}
}