using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace GitTrends.Shared
{
    public class IssuesConnection
    {
        [JsonConstructor, Obsolete]
        public IssuesConnection(int totalCount, List<Issue> nodes, [CallerMemberName] string unused = null) : this(totalCount, nodes)
        {

        }

        public IssuesConnection(int issuesCount, List<Issue> issueList) => (IssuesCount, IssueList) = (issuesCount, issueList);

        [JsonProperty("nodes")]
        public List<Issue> IssueList { get; }

        [JsonProperty("totalCount")]
        public int IssuesCount { get; }
    }
}
