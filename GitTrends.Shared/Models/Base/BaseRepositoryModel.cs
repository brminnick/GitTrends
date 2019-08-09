using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public abstract class BaseRepositoryModel
    {
        protected BaseRepositoryModel(long totalViewCount, long uniqueViewCount) =>
            (TotalCount, TotalUniqueCount) = (totalViewCount, uniqueViewCount);

        [JsonProperty("count")]
        public long TotalCount { get; }

        [JsonProperty("uniques")]
        public long TotalUniqueCount { get; }
    }
}
