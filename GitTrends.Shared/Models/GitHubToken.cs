using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class GitHubToken
    {
        public GitHubToken(string access_token, string scope, string token_type) =>
            (AccessToken, Scope, TokenType) = (access_token, scope, token_type);

        [JsonProperty("access_token")]
        public string AccessToken { get; }

        [JsonProperty("scope")]
        public string Scope { get; }

        [JsonProperty("token_type")]
        public string TokenType { get; }
    }
}
