using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class IssuesConnection
    {
        public IssuesConnection(long totalCount, IEnumerable<Issue>? nodes) =>
            (IssuesCount, IssueList) = (totalCount, nodes?.ToList() ?? (IReadOnlyList<Issue>)Array.Empty<Issue>());

        [JsonProperty("nodes")]
        public IReadOnlyList<Issue> IssueList { get; }

        [JsonProperty("totalCount")]
        public long IssuesCount { get; }
    }
}
