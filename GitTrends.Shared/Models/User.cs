using System;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace GitTrends.Shared
{
    public class User
    {
        [JsonConstructor, Obsolete]
        public User(RepositoryConnection repositories, string name, string company, DateTimeOffset createdAt, GitHubFollowers followers, [CallerMemberName]string unused = null)
            : this(repositories, name, company, createdAt, followers)
        {

        }

        public User(RepositoryConnection repositoryConnection, string name, string company, DateTimeOffset accountCreationDate, GitHubFollowers followers) =>
            (RepositoryConnection, Name, Company, AccountCreationDate, Followers) = (repositoryConnection, name, company, accountCreationDate, followers);

        [JsonProperty("repositories")]
        public RepositoryConnection RepositoryConnection { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("company")]
        public string Company { get; }

        [JsonProperty("createdAt")]
        public DateTimeOffset AccountCreationDate { get; }

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
