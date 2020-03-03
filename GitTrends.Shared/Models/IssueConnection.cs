using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class IssuesConnection
    {
        public IssuesConnection(int totalCount, IEnumerable<Issue>? nodes) =>
            (IssuesCount, IssueList) = (totalCount, nodes?.ToList() ?? Enumerable.Empty<Issue>().ToList());

        [JsonProperty("nodes")]
        public List<Issue> IssueList { get; }

        [JsonProperty("totalCount")]
        public int IssuesCount { get; }
    }
}
