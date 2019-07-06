using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class IssuesConnection
    {
        public IssuesConnection(int totalCount, List<Issue> nodes) => (IssuesCount, IssueList) = (totalCount, nodes);

        [JsonProperty("nodes")]
        public List<Issue> IssueList { get; }

        [JsonProperty("totalCount")]
        public int IssuesCount { get; }
    }
}
