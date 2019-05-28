using System;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class User
    {
        [JsonConstructor, Obsolete]
        public User(RepositoryConnection repositories, string name, string company, DateTimeOffset createdAt, string login, GitHubFollowers followers, [CallerMemberName]string unused = null)
            : this(repositories, name, company, createdAt, login, followers)
        {

        }

        public User(RepositoryConnection repositoryConnection, string name, string company, DateTimeOffset accountCreationDate, string alias, GitHubFollowers followers) =>
            (RepositoryConnection, Name, Company, AccountCreationDate, Alias, Followers) = (repositoryConnection, name, company, accountCreationDate, alias, followers);

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

        [JsonIgnore]
        public int FollowerCount => Followers?.Count ?? -1;

        [JsonProperty("followers")]
        GitHubFollowers Followers { get; }

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
        [JsonConstructor, Obsolete]
        public GitHubFollowers(int totalCount, [CallerMemberName]string unused = null) : this(totalCount)
        {

        }

        public GitHubFollowers(int count) => Count = count;

        [JsonProperty("totalCount")]
        public int Count { get; set; }
    }
}
