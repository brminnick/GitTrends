using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    abstract class BaseDailyModel
    {
        protected BaseDailyModel(DateTimeOffset day, long totalViews, long totalUniqueViews) =>
            (Day, TotalCount, TotalUniqueCount) = (day, totalViews, totalUniqueViews);

        [JsonProperty("timestamp")]
        public DateTimeOffset Day { get; }

        [JsonProperty("count")]
        protected long TotalCount { get; }

        [JsonProperty("uniques")]
        protected long TotalUniqueCount { get; }
    }
}
