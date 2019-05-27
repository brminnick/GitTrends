using System;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class Repository
    {
        [JsonConstructor, Obsolete]
        public Repository(string name, string description, long forkCount, RepositoryOwner owner, IssuesConnection issues, Uri url, StarGazers stargazers, [CallerMemberName]string unused = null)
            : this(name, description, forkCount, owner, issues, url, stargazers)
        {

        }

        public Repository(string name, string description, long forkCount, RepositoryOwner owner, IssuesConnection issuesConnection, Uri url, StarGazers stargazers) =>
            (Name, Description, ForkCount, Owner, Issues, Uri, StarGazers) = (name, description, forkCount, owner, issuesConnection, url, stargazers);


        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("forkCount")]
        public long ForkCount { get; }

        [JsonProperty("owner")]
        public RepositoryOwner Owner { get; }

        [JsonProperty("issues")]
        public IssuesConnection Issues { get; }

        [JsonProperty("url")]
        public Uri Uri { get; }

        [JsonIgnore]
        public int StarCount => StarGazers?.TotalCount ?? 0;

        [JsonProperty("stargazers")]
        StarGazers StarGazers { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
            stringBuilder.AppendLine($"{nameof(Owner)} {nameof(Owner.Login)}: {Owner?.Login}");
            stringBuilder.AppendLine($"{nameof(Owner)} {nameof(Owner.AvatarUrl)}: {Owner?.AvatarUrl}");
            stringBuilder.AppendLine($"{nameof(StarCount)}: {StarCount}");
            stringBuilder.AppendLine($"{nameof(Description)}: {Description}");
            stringBuilder.AppendLine($"{nameof(ForkCount)}: {ForkCount}");
            stringBuilder.AppendLine($"{nameof(Issues)}Count: {Issues?.IssueList.Count}");

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
