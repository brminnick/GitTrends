using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public abstract class BaseTotalCountModel
    {
        protected BaseTotalCountModel(long totalCount, long uniqueCount) =>
            (TotalCount, TotalUniqueCount) = (totalCount, uniqueCount);

        [JsonProperty("count")]
        public long TotalCount { get; }

        [JsonProperty("uniques")]
        public long TotalUniqueCount { get; }
    }
}
