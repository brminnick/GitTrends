using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using SQLite;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public class RepositoryDatabase : BaseDatabase
	{
		public RepositoryDatabase(IFileSystem fileSystem, IAnalyticsService analyticsService) : base(fileSystem, analyticsService, TimeSpan.FromDays(14))
		{
			GitHubApiRepositoriesService.RepositoryUriNotFound += HandleRepositoryUriNotFound;
		}

		public override async Task<int> DeleteAllData()
		{
			var (repositoryDatabaseConnection, dailyClonesDatabaseConnection, dailyViewsDatabaseConnection, starGazerInfoDatabaseConnection) = await GetDatabaseConnections().ConfigureAwait(false);

			await AttemptAndRetry(() => dailyViewsDatabaseConnection.DeleteAllAsync<DailyViewsDatabaseModel>()).ConfigureAwait(false);
			await AttemptAndRetry(() => dailyClonesDatabaseConnection.DeleteAllAsync<DailyClonesDatabaseModel>()).ConfigureAwait(false);
			await AttemptAndRetry(() => starGazerInfoDatabaseConnection.DeleteAllAsync<StarGazerInfoDatabaseModel>()).ConfigureAwait(false);
			return await AttemptAndRetry(() => repositoryDatabaseConnection.DeleteAllAsync<RepositoryDatabaseModel>()).ConfigureAwait(false);
		}

		public async Task DeleteExpiredData()
		{
			var (_, dailyClonesDatabaseConnection, dailyViewsDatabaseConnection, _) = await GetDatabaseConnections().ConfigureAwait(false);

			var dailyClones = await dailyClonesDatabaseConnection.Table<DailyClonesDatabaseModel>().ToListAsync();
			var dailyViews = await dailyViewsDatabaseConnection.Table<DailyViewsDatabaseModel>().ToListAsync();

			var expiredDailyClones = dailyClones.Where(x => IsExpired(x.DownloadedAt)).ToList();
			var expiredDailyViews = dailyViews.Where(x => IsExpired(x.DownloadedAt)).ToList();

			foreach (var expiredDailyClone in expiredDailyClones)
				await dailyClonesDatabaseConnection.DeleteAsync(expiredDailyClone).ConfigureAwait(false);

			foreach (var expiredDailyView in expiredDailyViews)
				await dailyViewsDatabaseConnection.DeleteAsync(expiredDailyView).ConfigureAwait(false);
		}

		public async Task DeleteRepository(Repository repository)
		{
			var (repositoryDatabaseConnection,
					dailyClonesDatabaseConnection,
					dailyViewsDatabaseConnection,
					starGazerInfoDatabaseConnection) = await GetDatabaseConnections().ConfigureAwait(false);

			var dailyViews = await dailyViewsDatabaseConnection.Table<DailyViewsDatabaseModel>().ToListAsync();
			var dailyClones = await dailyClonesDatabaseConnection.Table<DailyClonesDatabaseModel>().ToListAsync();
			var starGazerInfos = await starGazerInfoDatabaseConnection.Table<StarGazerInfoDatabaseModel>().ToListAsync();

			foreach (var dailyClone in dailyClones.Where(x => x.RepositoryUrl == repository.Url))
				await dailyClonesDatabaseConnection.DeleteAsync(dailyClone).ConfigureAwait(false);

			foreach (var dailyView in dailyViews.Where(x => x.RepositoryUrl == repository.Url))
				await dailyViewsDatabaseConnection.DeleteAsync(dailyView).ConfigureAwait(false);

			foreach (var starGazerInfo in starGazerInfos.Where(x => x.RepositoryUrl == repository.Url))
				await starGazerInfoDatabaseConnection.DeleteAsync(starGazerInfo).ConfigureAwait(false);

			await repositoryDatabaseConnection.DeleteAsync(RepositoryDatabaseModel.ToRepositoryDatabase(repository)).ConfigureAwait(false);
		}

		public async Task SaveRepository(Repository repository)
		{
			var databaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

			var repositoryDatabaseModel = RepositoryDatabaseModel.ToRepositoryDatabase(repository);
			await AttemptAndRetry(() => databaseConnection.InsertOrReplaceAsync(repositoryDatabaseModel)).ConfigureAwait(false);

			await SaveStarGazerInfo(repository).ConfigureAwait(false);
			await SaveDailyClones(repository).ConfigureAwait(false);
			await SaveDailyViews(repository).ConfigureAwait(false);
		}

		public async Task<IReadOnlyList<string>> GetFavoritesUrls()
		{
			var repositoryDatabaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);
			var favoriteRepositories = await AttemptAndRetry(() => repositoryDatabaseConnection.Table<RepositoryDatabaseModel>().Where(x => x.IsFavorite == true).ToListAsync()).ConfigureAwait(false);

			return favoriteRepositories.Select(x => x.Url).ToList();
		}

		public async Task<Repository?> GetRepository(string repositoryUrl)
		{
			var repositoryDatabaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

			var repositoryDatabaseModel = await AttemptAndRetry(() => repositoryDatabaseConnection.Table<RepositoryDatabaseModel>().FirstOrDefaultAsync(x => x.Url == repositoryUrl)).ConfigureAwait(false);
			if (repositoryDatabaseModel is null)
				return null;

			var (dailyClones, dailyViews, starGazers) = await GetClonesViewsStars(repositoryDatabaseModel).ConfigureAwait(false);

			return RepositoryDatabaseModel.ToRepository(repositoryDatabaseModel, dailyClones, dailyViews, starGazers);
		}

		public async Task<IReadOnlyList<Repository>> GetRepositories()
		{
			var repositoryDatabaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

			var repositoryDatabaseModels = await AttemptAndRetry(() => repositoryDatabaseConnection.Table<RepositoryDatabaseModel>().ToListAsync()).ConfigureAwait(false);
			if (!repositoryDatabaseModels.Any())
				return Array.Empty<Repository>();

			var repositoryList = new List<Repository>();
			foreach (var repositoryDatabaseModel in repositoryDatabaseModels)
			{
				var (dailyClones, dailyViews, starGazers) = await GetClonesViewsStars(repositoryDatabaseModel).ConfigureAwait(false);

				var repository = RepositoryDatabaseModel.ToRepository(repositoryDatabaseModel, dailyClones, dailyViews, starGazers);
				repositoryList.Add(repository);
			}

			return repositoryList;
		}

		static bool IsWithin14Days(DateTimeOffset dataDate, DateTimeOffset mostRecentDate) => dataDate.CompareTo(mostRecentDate.Subtract(TimeSpan.FromDays(13)).ToLocalTime()) >= 0;

		async Task<(IReadOnlyList<DailyClonesDatabaseModel>? dailyClones,
							IReadOnlyList<DailyViewsDatabaseModel>? dailyViews,
							IReadOnlyList<StarGazerInfoDatabaseModel>? starGazers)> GetClonesViewsStars(RepositoryDatabaseModel repository)
		{
			var (_, dailyClonesDatabaseConnection, dailyViewsDatabaseConnection, starGazerInfoDatabaseConnection) = await GetDatabaseConnections().ConfigureAwait(false);

			var getStarGazerInfoModelsTask = AttemptAndRetry(() => starGazerInfoDatabaseConnection.Table<StarGazerInfoDatabaseModel>().Where(x => x.RepositoryUrl == repository.Url).ToListAsync());
			var getDailyClonesDatabaseModelsTask = AttemptAndRetry(() => dailyClonesDatabaseConnection.Table<DailyClonesDatabaseModel>().Where(x => x.RepositoryUrl == repository.Url).ToListAsync());
			var getDailyViewsDatabaseModelsTask = AttemptAndRetry(() => dailyViewsDatabaseConnection.Table<DailyViewsDatabaseModel>().Where(x => x.RepositoryUrl == repository.Url).ToListAsync());

			await Task.WhenAll(getDailyClonesDatabaseModelsTask, getDailyViewsDatabaseModelsTask, getStarGazerInfoModelsTask).ConfigureAwait(false);

			var starGazerInfoModels = await getStarGazerInfoModelsTask.ConfigureAwait(false);
			var dailyClonesDatabaseModels = await getDailyClonesDatabaseModelsTask.ConfigureAwait(false);
			var dailyViewsDatabaseModels = await getDailyViewsDatabaseModelsTask.ConfigureAwait(false);

			if (!starGazerInfoModels.Any())
				starGazerInfoModels = null;

			if (!dailyClonesDatabaseModels.Any())
				dailyClonesDatabaseModels = null;

			if (!dailyViewsDatabaseModels.Any())
				dailyViewsDatabaseModels = null;

			var sortedRecentDailyClonesDatabaseModels = dailyClonesDatabaseModels?.OrderByDescending(x => x.DownloadedAt).ToList();
			var sortedRecentDailyViewsDatabaseModels = dailyViewsDatabaseModels?.OrderByDescending(x => x.DownloadedAt).ToList();
			var sortedStarGazerInfoModels = starGazerInfoModels?.OrderByDescending(x => x.StarredAt).ToList();

			var mostRecentCloneDay = sortedRecentDailyClonesDatabaseModels?.Any() is true ? sortedRecentDailyClonesDatabaseModels.Max(x => x.Day) : default;
			var mostRecentViewDay = sortedRecentDailyViewsDatabaseModels?.Any() is true ? sortedRecentDailyViewsDatabaseModels.Max(x => x.Day) : default;

			var mostRecentDate = mostRecentCloneDay.CompareTo(mostRecentViewDay) > 0 ? mostRecentCloneDay : mostRecentViewDay;

			var dailyClones = sortedRecentDailyClonesDatabaseModels?.Where(x => IsWithin14Days(x.Day, mostRecentDate)).GroupBy(x => x.Day).Select(x => x.First()).Take(14);
			var dailyViews = sortedRecentDailyViewsDatabaseModels?.Where(x => IsWithin14Days(x.Day, mostRecentDate)).GroupBy(x => x.Day).Select(x => x.First()).Take(14);

			return (dailyClones?.ToList(), dailyViews?.ToList(), sortedStarGazerInfoModels);
		}

		async Task<(SQLiteAsyncConnection RepositoryDatabaseConnection,
						SQLiteAsyncConnection DailyClonesDatabaseConnection,
						SQLiteAsyncConnection DailyViewsDatabaseConnection,
						SQLiteAsyncConnection StarGazerInfoDatabaseConnection)> GetDatabaseConnections()
		{
			var repositoryDatabaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);
			var dailyViewsDatabaseConnection = await GetDatabaseConnection<DailyViewsDatabaseModel>().ConfigureAwait(false);
			var dailyClonesDatabaseConnection = await GetDatabaseConnection<DailyClonesDatabaseModel>().ConfigureAwait(false);
			var starGazerInfoDatabaseConnection = await GetDatabaseConnection<StarGazerInfoDatabaseModel>().ConfigureAwait(false);

			return (repositoryDatabaseConnection, dailyClonesDatabaseConnection, dailyViewsDatabaseConnection, starGazerInfoDatabaseConnection);
		}

		async Task SaveStarGazerInfo(Repository repository)
		{
			if (repository.StarredAt is null)
				return;

			var starGazerInfoDatabaseConnection = await GetDatabaseConnection<StarGazerInfoDatabaseModel>().ConfigureAwait(false);

			foreach (var starredAtDate in repository.StarredAt)
			{
				var starGazerInfoDatabaseModel = new StarGazerInfoDatabaseModel
				{
					RepositoryUrl = repository.Url,
					StarredAt = starredAtDate
				};

				await AttemptAndRetry(() => starGazerInfoDatabaseConnection.InsertOrReplaceAsync(starGazerInfoDatabaseModel)).ConfigureAwait(false);
			}
		}

		async Task SaveDailyClones(Repository repository)
		{
			if (repository.DailyClonesList is null)
				return;

			var dailyClonesDatabaseConnection = await GetDatabaseConnection<DailyClonesDatabaseModel>().ConfigureAwait(false);

			foreach (var dailyClonesModel in repository.DailyClonesList)
			{
				var dailyClonesDatabaseModel = DailyClonesDatabaseModel.ToDailyClonesDatabaseModel(dailyClonesModel, repository);
				await AttemptAndRetry(() => dailyClonesDatabaseConnection.InsertOrReplaceAsync(dailyClonesDatabaseModel)).ConfigureAwait(false);
			}
		}

		async Task SaveDailyViews(Repository repository)
		{
			if (repository.DailyViewsList is null)
				return;

			var dailyViewsDatabaseConnection = await GetDatabaseConnection<DailyViewsDatabaseModel>().ConfigureAwait(false);

			foreach (var dailyViewsModel in repository.DailyViewsList)
			{
				var dailyViewsDatabaseModel = DailyViewsDatabaseModel.ToDailyViewsDatabaseModel(dailyViewsModel, repository);
				await AttemptAndRetry(() => dailyViewsDatabaseConnection.InsertOrReplaceAsync(dailyViewsDatabaseModel)).ConfigureAwait(false);
			}
		}

		async void HandleRepositoryUriNotFound(object sender, Uri e)
		{
			var repositoryFromDatabase = await GetRepository(e.ToString()).ConfigureAwait(false);

			if (repositoryFromDatabase is not null)
				await DeleteRepository(repositoryFromDatabase).ConfigureAwait(false);
		}

		record DailyClonesDatabaseModel : IDailyClonesModel
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
				var clonesList = dailyClonesDatabaseModels?.Select(x => DailyClonesDatabaseModel.ToDailyClonesModel(x)).ToList();
				var viewsList = dailyViewsDatabaseModels?.Select(x => DailyViewsDatabaseModel.ToDailyViewsModel(x)).ToList();

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
										repositoryDatabaseModel.IsFavorite,
										viewsList,
										clonesList,
										starGazerInfoDatabaseModels?.Select(x => x.StarredAt).Distinct());
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
}