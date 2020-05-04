using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using SQLite;
using Xamarin.Essentials;

namespace GitTrends
{
    public abstract class BaseDatabase
    {
        static readonly string _databasePath = Path.Combine(FileSystem.AppDataDirectory, $"{nameof(GitTrends)}.db3");

        static readonly Lazy<SQLiteAsyncConnection> _databaseConnectionHolder =
            new Lazy<SQLiteAsyncConnection>(() => new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache));

        protected abstract TimeSpan Expiration { get; }

        static SQLiteAsyncConnection DatabaseConnection => _databaseConnectionHolder.Value;

        protected bool IsExpired(DateTimeOffset downloadedAt) => downloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(Expiration)) < 0;

        protected static async ValueTask<SQLiteAsyncConnection> GetDatabaseConnection<T>()
        {
            if (!DatabaseConnection.TableMappings.Any(x => x.MappedType == typeof(T)))
            {
                await DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
                await DatabaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
            }

            return DatabaseConnection;
        }

        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 12)
        {
            return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);

            static TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromMilliseconds(Math.Pow(2, attemptNumber));
        }
    }
}
