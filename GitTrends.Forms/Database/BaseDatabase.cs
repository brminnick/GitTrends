using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using Polly;
using SQLite;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public abstract class BaseDatabase
	{
		readonly SQLiteAsyncConnection _databaseConnection;

		protected BaseDatabase(IFileSystem fileSystem, IAnalyticsService analyticsService, TimeSpan expiresAt)
		{
			ExpiresAt = expiresAt;
			AnalyticsService = analyticsService;

			var databasePath = Path.Combine(fileSystem.AppDataDirectory, $"{nameof(GitTrends)}.db3");
			_databaseConnection = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
		}

		public TimeSpan ExpiresAt { get; }
		protected IAnalyticsService AnalyticsService { get; }

		public abstract Task<int> DeleteAllData();

		protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 12)
		{
			return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);

			static TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromMilliseconds(Math.Pow(2, attemptNumber));
		}

		protected bool IsExpired(in DateTimeOffset downloadedAt) => downloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(ExpiresAt)) <= 0;

		protected async ValueTask<SQLiteAsyncConnection> GetDatabaseConnection<T>()
		{
			if (!_databaseConnection.TableMappings.Any(static x => x.MappedType == typeof(T)))
			{
				await _databaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);

				try
				{
					await _databaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
				}
				catch (SQLiteException e) when (e.Message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase))
				{
					await _databaseConnection.DropTableAsync(_databaseConnection.TableMappings.First(static x => x.MappedType == typeof(T)));
					await _databaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
				}
			}

			return _databaseConnection;
		}
	}
}