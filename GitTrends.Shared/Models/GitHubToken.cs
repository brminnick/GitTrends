using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class GitHubToken
    {
        public GitHubToken(string accessToken, string scope, string tokenType) =>
            (AccessToken, Scope, TokenType) = (accessToken, scope, tokenType);

        [JsonProperty("access_token")]
        public string AccessToken { get; }

        [JsonProperty("scope")]
        public string Scope { get; }

        [JsonProperty("token_type")]
        public string TokenType { get; }
    }
}
