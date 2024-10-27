using GitTrends.Common;
using Polly;
using Polly.Retry;
using SQLite;

namespace GitTrends;

public abstract class BaseDatabase
{
	readonly Lazy<SQLiteAsyncConnection> _databaseConnectionHolder;

	protected BaseDatabase(IFileSystem fileSystem, IAnalyticsService analyticsService, TimeSpan expiresAt)
	{
		ExpiresAt = expiresAt;
		AnalyticsService = analyticsService;

		var databasePath = Path.Combine(fileSystem.AppDataDirectory, $"{nameof(GitTrends)}.db3");
		_databaseConnectionHolder = new(() => new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache));
	}

	public TimeSpan ExpiresAt { get; }
	protected IAnalyticsService AnalyticsService { get; }

	SQLiteAsyncConnection DatabaseConnection => _databaseConnectionHolder.Value;

	public abstract Task DeleteAllData(CancellationToken token);

	protected async Task<TReturn> Execute<TReturn, TDatabase>(Func<SQLiteAsyncConnection, Task<TReturn>> action, CancellationToken token, int maxRetries = 10)
	{
		var databaseConnection = await GetDatabaseConnection<TDatabase>().ConfigureAwait(false);

		var resiliencePipeline = new ResiliencePipelineBuilder<TReturn>()
			.AddRetry(new RetryStrategyOptions<TReturn>
			{
				MaxRetryAttempts = maxRetries,
				Delay = TimeSpan.FromMilliseconds(2),
				BackoffType = DelayBackoffType.Exponential
			}).Build();

		return await resiliencePipeline.ExecuteAsync(async _ => await action(databaseConnection), token).ConfigureAwait(false);
	}

	protected bool IsExpired(in DateTimeOffset downloadedAt) => downloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(ExpiresAt)) <= 0;

	async ValueTask<SQLiteAsyncConnection> GetDatabaseConnection<T>()
	{
		if (DatabaseConnection.TableMappings.Any(static x => x.MappedType == typeof(T)))
			return DatabaseConnection;

		await DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
		await DatabaseConnection.CreateTableAsync(typeof(T)).ConfigureAwait(false);

		return DatabaseConnection;
	}
}