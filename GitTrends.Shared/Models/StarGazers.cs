using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class StarGazers
    {
        public StarGazers(long totalCount, IEnumerable<StarGazerInfo> edges) =>
            (TotalCount, StarredAt) = (totalCount, edges.ToList());

        [JsonProperty("totalCount")]
        public long TotalCount { get; }

        [JsonProperty("edges")]
        public IReadOnlyList<StarGazerInfo> StarredAt { get; }
    }

    public class StarGazerInfo
    {
        public StarGazerInfo(DateTimeOffset starredAt, string cursor) =>
            (StarredAt, Cursor) = (starredAt, cursor);

        public DateTimeOffset StarredAt { get; }
        public string Cursor { get; }
    }
}
