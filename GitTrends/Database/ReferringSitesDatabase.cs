using GitTrends.Common;
using SQLite;

namespace GitTrends;

public class ReferringSitesDatabase(IFileSystem fileSystem, IAnalyticsService analyticsService) : BaseDatabase(fileSystem, analyticsService, CachedDataConstants.DatabaseReferringSitesLifeSpan)
{
	public override Task<int> DeleteAllData(CancellationToken token) =>
		Execute<int, MobileReferringSitesDatabaseModel>(static databaseConnection => databaseConnection.DeleteAllAsync<MobileReferringSitesDatabaseModel>(), token);

	public Task DeleteExpiredData(CancellationToken token) =>
		Execute<int, MobileReferringSitesDatabaseModel>(async databaseConnection =>
		{
			var referringSites = await databaseConnection.Table<MobileReferringSitesDatabaseModel>().ToListAsync();
			var expiredReferringSites = referringSites.Where(x => IsExpired(x.DownloadedAt));

			foreach (var expiredReferringSite in expiredReferringSites)
				await databaseConnection.DeleteAsync(expiredReferringSite).ConfigureAwait(false);

			return 0;

		}, token);

	public Task<MobileReferringSiteModel?> GetReferringSite(string repositoryUrl, Uri? referrerUri, CancellationToken token) =>
		Execute<MobileReferringSiteModel?, MobileReferringSitesDatabaseModel>(async databaseConnection =>
		{
			var referringSitesDatabaseModelList = await databaseConnection.Table<MobileReferringSitesDatabaseModel>().Where(x => x.RepositoryUrl == repositoryUrl && x.ReferrerUri == referrerUri).ToListAsync().ConfigureAwait(false);

			var newestReferringSiteModel = referringSitesDatabaseModelList.MaxBy(static x => x.DownloadedAt);

			try
			{
				return newestReferringSiteModel is null ? null : MobileReferringSitesDatabaseModel.ToReferringSitesModel(newestReferringSiteModel);
			}
			catch (UriFormatException)
			{
				await databaseConnection.DeleteAsync(newestReferringSiteModel).ConfigureAwait(false);
				return null;
			}
		}, token);

	public Task<List<MobileReferringSiteModel>> GetReferringSites(string repositoryUrl, CancellationToken token) =>
		Execute<List<MobileReferringSiteModel>, MobileReferringSitesDatabaseModel>(async databaseConnection =>
		{
			var referringSitesDatabaseModelList = await databaseConnection.Table<MobileReferringSitesDatabaseModel>().Where(x => x.RepositoryUrl == repositoryUrl).ToListAsync().ConfigureAwait(false);

			return [.. referringSitesDatabaseModelList.Select(static x => MobileReferringSitesDatabaseModel.ToReferringSitesModel(x))];
		}, token);

	public Task<int> SaveReferringSite(MobileReferringSiteModel referringSiteModel, string repositoryUrl, CancellationToken token) =>
		Execute<int, MobileReferringSitesDatabaseModel>(async databaseConnection =>
		{
			var referringSitesDatabaseModel = MobileReferringSitesDatabaseModel.ToReferringSitesDatabaseModel(referringSiteModel, repositoryUrl);

			return await databaseConnection.InsertOrReplaceAsync(referringSitesDatabaseModel).ConfigureAwait(false);
		}, token);

	sealed record MobileReferringSitesDatabaseModel : IMobileReferringSiteModel
	{
		//PrimaryKey must be nullable https://github.com/praeclarum/sqlite-net/issues/327
		[PrimaryKey]
		public int? Id { get; init; }

		[Indexed]
		public string RepositoryUrl { get; init; } = string.Empty;

		public string FavIconImageUrl { get; init; } = string.Empty;

		public DateTimeOffset DownloadedAt { get; init; }

		public string Referrer { get; init; } = string.Empty;

		public bool IsReferrerUriValid { get; init; }

		public Uri? ReferrerUri { get; init; }

		public long TotalCount { get; init; }

		public long TotalUniqueCount { get; init; }

		public static MobileReferringSiteModel ToReferringSitesModel(MobileReferringSitesDatabaseModel referringSitesDatabaseModel)
		{
			var referringSiteModel = new ReferringSiteModel(referringSitesDatabaseModel.TotalCount, referringSitesDatabaseModel.TotalUniqueCount, referringSitesDatabaseModel.Referrer)
			{
				DownloadedAt = referringSitesDatabaseModel.DownloadedAt
			};

			try
			{
				return new MobileReferringSiteModel(referringSiteModel, string.IsNullOrWhiteSpace(referringSitesDatabaseModel.FavIconImageUrl) ? null : ImageSource.FromUri(new Uri(referringSitesDatabaseModel.FavIconImageUrl)));
			}
			catch (UriFormatException)
			{
				return new MobileReferringSiteModel(referringSiteModel, ImageSource.FromFile(referringSitesDatabaseModel.FavIconImageUrl));
			}
		}

		public static MobileReferringSitesDatabaseModel ToReferringSitesDatabaseModel(MobileReferringSiteModel referringSiteModel, string repositoryUrl)
		{
			return new MobileReferringSitesDatabaseModel
			{
				FavIconImageUrl = referringSiteModel.FavIconImageUrl,
				DownloadedAt = referringSiteModel.DownloadedAt,
				RepositoryUrl = repositoryUrl,
				Referrer = referringSiteModel.Referrer,
				TotalUniqueCount = referringSiteModel.TotalUniqueCount,
				IsReferrerUriValid = referringSiteModel.IsReferrerUriValid,
				ReferrerUri = referringSiteModel.ReferrerUri,
				TotalCount = referringSiteModel.TotalCount
			};
		}
	}
}