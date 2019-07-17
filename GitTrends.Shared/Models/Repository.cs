using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GitTrends.Shared
{
    public class Repository : IRepository
    {
        public Repository(string name, string description, long forkCount, RepositoryOwner owner, IssuesConnection issues, Uri url, StarGazers stargazers) =>
            (Name, Description, ForkCount, OwnerLogin, OwnerAvatarUrl, IssuesCount, Uri, StarCount) = (name, description, forkCount, owner.Login, owner.AvatarUrl, issues?.IssuesCount ?? 0, url, stargazers.TotalCount);

        public string OwnerLogin { get; }
        public Uri OwnerAvatarUrl { get; }
        public int StarCount { get; }
        public int IssuesCount { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("forkCount")]
        public long ForkCount { get; }

        [JsonProperty("url")]
        public Uri Uri { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
            stringBuilder.AppendLine($"{nameof(OwnerLogin)}: {OwnerLogin}");
            stringBuilder.AppendLine($"{nameof(OwnerAvatarUrl)}: {OwnerAvatarUrl}");
            stringBuilder.AppendLine($"{nameof(StarCount)}: {StarCount}");
            stringBuilder.AppendLine($"{nameof(Description)}: {Description}");
            stringBuilder.AppendLine($"{nameof(ForkCount)}: {ForkCount}");
            stringBuilder.AppendLine($"{nameof(IssuesCount)}: {IssuesCount}");

            return stringBuilder.ToString();
        }
    }

    public class RepositoryOwner
    {
        public RepositoryOwner(string login, Uri avatarUrl) => (Login, AvatarUrl) = (login, avatarUrl);

        [JsonProperty("login")]
        public string Login { get; }

        [JsonProperty("avatarUrl")]
        public Uri AvatarUrl { get; }
    }

    public class StarGazers
    {
        public StarGazers(int totalCount) => TotalCount = totalCount;

        [JsonProperty("totalCount")]
        public int TotalCount { get; }
    }
}
