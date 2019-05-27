using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace GitTrends.Shared
{
    public class RepositoryConnection
    {
        [JsonConstructor, Obsolete]
        public RepositoryConnection(List<Repository> nodes, PageInfo pageInfo, [CallerMemberName]string unused = null) : this(nodes, pageInfo)
        {

        }

        public RepositoryConnection(List<Repository> repositoryList, PageInfo pageInfo) =>
            (RepositoryList, PageInfo) = (repositoryList, pageInfo);

        [JsonProperty("nodes")]
        public IEnumerable<Repository> RepositoryList { get; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; }
    }
}
