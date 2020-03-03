using System;
using System.Text;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class User
    {
        public User(RepositoryConnection repositories, string name, string company, DateTimeOffset createdAt, string login, Uri avatarUrl, GitHubFollowers? followers)
        {
            RepositoryConnection = repositories;
            Name = name;
            Company = company;
            AccountCreationDate = createdAt;
            Alias = login;
            AvatarUri = avatarUrl;
            FollowerCount = followers?.Count ?? 0;
        }

        [JsonProperty("repositories")]
        public RepositoryConnection RepositoryConnection { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("company")]
        public string Company { get; }

        [JsonProperty("createdAt")]
        public DateTimeOffset AccountCreationDate { get; }

        [JsonProperty("login")]
        public string Alias { get; }

        [JsonProperty("avatarUrl")]
        public Uri AvatarUri { get; }

        public int FollowerCount { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
            stringBuilder.AppendLine($"{nameof(Company)}: {Company}");
            stringBuilder.AppendLine($"{nameof(FollowerCount)}: {FollowerCount}");
            stringBuilder.AppendLine($"{nameof(AccountCreationDate)}: {AccountCreationDate}");

            return stringBuilder.ToString();
        }
    }

    public class GitHubFollowers
    {
        public GitHubFollowers(int totalCount) => Count = totalCount;

        [JsonProperty("totalCount")]
        public int Count { get; }
    }
}
