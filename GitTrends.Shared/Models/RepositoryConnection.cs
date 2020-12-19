using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public record RepositoryConnection
    {
        public RepositoryConnection(IEnumerable<RepositoryConnectionNode>? nodes, PageInfo pageInfo)
        {
            RepositoryList = (nodes ?? Array.Empty<RepositoryConnectionNode>()).ToList();
            PageInfo = pageInfo;
        }

        [JsonProperty("nodes")]
        public IReadOnlyList<RepositoryConnectionNode?> RepositoryList { get; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; }
    }

    public record RepositoryConnectionNode(string Name, string Description, long ForkCount, Uri Url, RepositoryOwner Owner, bool IsFork, IssuesConnection Issues)
    {
        public DateTimeOffset DataDownloadedAt { get; } = DateTimeOffset.UtcNow;
    }

    public record RepositoryOwner(string Login, string AvatarUrl);
}
