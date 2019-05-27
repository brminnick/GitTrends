using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class GitHubToken
    {
        public GitHubToken(string access_Token, string scope, string token_Type) =>
            (AccessToken, Scope, TokenType) = (access_Token, scope, token_Type);

        [JsonProperty("access_token")]
        public string AccessToken { get; }

        [JsonProperty("scope")]
        public string Scope { get; }

        [JsonProperty("token_type")]
        public string TokenType { get; }
    }
}
