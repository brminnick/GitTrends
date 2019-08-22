using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryConnection
    {
        public RepositoryConnection(IEnumerable<Repository> nodes, PageInfo pageInfo) =>
            (RepositoryList, PageInfo) = (nodes?.ToList() ?? Enumerable.Empty<Repository>().ToList(), pageInfo);

        [JsonProperty("nodes")]
        public List<Repository> RepositoryList { get; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; }
    }
}
