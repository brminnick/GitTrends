using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryConnection
    {
        public RepositoryConnection(IEnumerable<RepositoryConnectionNode>? nodes, PageInfo pageInfo)
        {
            RepositoryList = nodes?.ToList() ?? (IReadOnlyList<RepositoryConnectionNode>)Array.Empty<RepositoryConnectionNode>();
            PageInfo = pageInfo;
        }

        [JsonProperty("nodes")]
        public IReadOnlyList<RepositoryConnectionNode> RepositoryList { get; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; }
    }

    public class RepositoryConnectionNode
    {
        public RepositoryConnectionNode(string name, string description, long forkCount, Uri url, RepositoryOwner owner, bool isFork, IssuesConnection issues)
        {
            DataDownloadedAt = DateTimeOffset.UtcNow;
            Name = name;
            Description = description;
            ForkCount = forkCount;
            Url = url;
            Owner = owner;
            IsFork = isFork;
            Issues = issues;
        }

        public DateTimeOffset DataDownloadedAt { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("forkCount")]
        public long ForkCount { get; }

        [JsonProperty("url")]
        public Uri Url { get; }

        [JsonProperty("owner")]
        public RepositoryOwner Owner { get; }

        [JsonProperty("isFork")]
        public bool IsFork { get; }

        [JsonProperty("issues")]
        public IssuesConnection Issues { get; }
    }

    public class RepositoryOwner
    {
        public RepositoryOwner(string login, string avatarUrl) => (Login, AvatarUrl) = (login, avatarUrl);

        [JsonProperty("login")]
        public string Login { get; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; }
    }
}
