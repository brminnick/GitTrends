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
        protected BaseDatabase(IFileSystem fileSystem, IAnalyticsService analyticsService, TimeSpan expiresAt)
        {
            ExpiresAt = expiresAt;
            AnalyticsService = analyticsService;

            var databasePath = Path.Combine(fileSystem.AppDataDirectory, $"{nameof(GitTrends)}.db3");
            DatabaseConnection = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        }

        public TimeSpan ExpiresAt { get; }
        protected IAnalyticsService AnalyticsService { get; }

        SQLiteAsyncConnection DatabaseConnection { get; }

        public abstract Task<int> DeleteAllData();

        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 12)
        {
            return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);

            static TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromMilliseconds(Math.Pow(2, attemptNumber));
        }

        protected bool IsExpired(in DateTimeOffset downloadedAt) => downloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(ExpiresAt)) <= 0;

        protected async ValueTask<SQLiteAsyncConnection> GetDatabaseConnection<T>()
        {
            if (!DatabaseConnection.TableMappings.Any(x => x.MappedType == typeof(T)))
            {
                await DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);

                try
                {
                    await DatabaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
                }
                catch (SQLiteException e) when (e.Message.Contains("PRIMARY KEY"))
                {
                    await DatabaseConnection.DropTableAsync(DatabaseConnection.TableMappings.First(x => x.MappedType == typeof(T)));
                    await DatabaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
                }
            }

            return DatabaseConnection;
        }
    }
}
