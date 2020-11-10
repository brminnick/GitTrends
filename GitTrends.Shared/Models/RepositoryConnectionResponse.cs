using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public record RepositoryConnectionResponse
    {
        public RepositoryConnectionResponse(User user) => GitHubUser = user;

        [JsonProperty("user")]
        public User GitHubUser { get; }
    }
}
