using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using SQLite;

namespace GitTrends
{
    public class RepositoryDatabase : BaseDatabase
    {
        public async Task<int> DeleteAllData()
        {
            var databaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

            return await AttemptAndRetry(() => databaseConnection.DeleteAllAsync<RepositoryDatabaseModel>()).ConfigureAwait(false);
        }

        public async Task<int> SaveRepository(Repository repository)
        {
            var databaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

            return await AttemptAndRetry(() => databaseConnection.InsertOrReplaceAsync((RepositoryDatabaseModel)repository)).ConfigureAwait(false);
        }

        public async Task<int> SaveRepositories(IEnumerable<Repository> repositories)
        {
            try
            {
                var databaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

                var repositoryDatabaseModels = repositories.Select(x => (RepositoryDatabaseModel)x);

                return await AttemptAndRetry(() => databaseConnection.InsertAllAsync(repositoryDatabaseModels)).ConfigureAwait(false);
            }
            catch (SQLiteException e) when (e.Result is SQLite3.Result.Constraint)
            {
                int count = 0;

                foreach (var repository in repositories)
                {
                    count += await SaveRepository(repository).ConfigureAwait(false);
                }

                return count;
            }
        }

        public async Task<Repository> GetRepository(Uri repositoryUri)
        {
            var databaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

            var repositoryDatabaseModel = await AttemptAndRetry(() => databaseConnection.GetAsync<RepositoryDatabaseModel>(repositoryUri)).ConfigureAwait(false);

            return (Repository)repositoryDatabaseModel;
        }

        public async Task<IEnumerable<Repository>> GetRepositories()
        {
            var databaseConnection = await GetDatabaseConnection<RepositoryDatabaseModel>().ConfigureAwait(false);

            var repositoryDatabaseModels = await AttemptAndRetry(() => databaseConnection.Table<RepositoryDatabaseModel>().ToListAsync()).ConfigureAwait(false);

            return repositoryDatabaseModels.Select(x => (Repository)x);
        }

        class RepositoryDatabaseModel : IRepository
        {
            public DateTimeOffset DataDownloadedAt { get; set; } = DateTimeOffset.UtcNow;

            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public long ForkCount { get; set; }

            [PrimaryKey]
            public string Url { get; set; } = string.Empty;

            public int StarCount { get; set; }

            public string OwnerLogin { get; set; } = string.Empty;

            public string OwnerAvatarUrl { get; set; } = string.Empty;

            public int IssuesCount { get; set; }

            public bool IsFork { get; set; }

            public long TotalViews { get; set; }

            public long TotalUniqueViews { get; set; }

            public long TotalClones { get; set; }

            public long TotalUniqueClones { get; set; }

            public static explicit operator Repository(RepositoryDatabaseModel repositoryDatabaseModel)
            {
                return new Repository(repositoryDatabaseModel.Name,
                                        repositoryDatabaseModel.Description,
                                        repositoryDatabaseModel.ForkCount,
                                        new RepositoryOwner(repositoryDatabaseModel.OwnerLogin, repositoryDatabaseModel.OwnerAvatarUrl ?? repositoryDatabaseModel.OwnerLogin),
                                        new IssuesConnection(repositoryDatabaseModel.IssuesCount, Enumerable.Empty<Issue>()),
                                        repositoryDatabaseModel.Url,
                                        new StarGazers(repositoryDatabaseModel.StarCount),
                                        repositoryDatabaseModel.IsFork,
                                        repositoryDatabaseModel.DataDownloadedAt);
            }

            public static implicit operator RepositoryDatabaseModel(Repository repository)
            {
                return new RepositoryDatabaseModel
                {
                    DataDownloadedAt = repository.DataDownloadedAt,
                    Description = repository.Description,
                    StarCount = repository.StarCount,
                    Url = repository.Url,
                    IssuesCount = repository.IssuesCount,
                    ForkCount = repository.ForkCount,
                    Name = repository.Name,
                    OwnerAvatarUrl = repository.OwnerAvatarUrl,
                    OwnerLogin = repository.OwnerLogin,
                    IsFork = repository.IsFork,
                    TotalClones = repository.TotalClones,
                    TotalUniqueClones = repository.TotalUniqueClones,
                    TotalViews = repository.TotalViews,
                    TotalUniqueViews = repository.TotalUniqueViews,
                };
            }
        }
    }
}
