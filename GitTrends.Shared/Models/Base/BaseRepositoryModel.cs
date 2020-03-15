using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public abstract class BaseRepositoryModel
    {
        protected BaseRepositoryModel(string repositoryName, string repositoryOwner, long totalViewCount, long uniqueViewCount) : this(totalViewCount, uniqueViewCount) =>
            (RepositoryName, RepositoryOwner) = (repositoryName, repositoryOwner);

        [JsonConstructor]
        protected BaseRepositoryModel(long totalViewCount, long uniqueViewCount) =>
            (TotalCount, TotalUniqueCount) = (totalViewCount, uniqueViewCount);

        [JsonProperty("count")]
        public long TotalCount { get; }

        [JsonProperty("uniques")]
        public long TotalUniqueCount { get; }

        public string RepositoryName { get; } = string.Empty;
        public string RepositoryOwner { get; } = string.Empty;
    }
}
