using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryConnection
    {
        public RepositoryConnection(List<Repository> nodes, PageInfo pageInfo) =>
            (RepositoryList, PageInfo) = (nodes, pageInfo);

        [JsonProperty("nodes")]
        public IEnumerable<Repository> RepositoryList { get; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; }
    }
}
