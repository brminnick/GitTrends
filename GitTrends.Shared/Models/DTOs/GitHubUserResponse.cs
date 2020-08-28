using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class GitHubUserResponse
    {
        public GitHubUserResponse(User user) => User = user;

        [JsonProperty("user")]
        public User User { get; }
    }
}
