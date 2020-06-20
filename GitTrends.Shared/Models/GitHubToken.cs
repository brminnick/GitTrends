using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class GitHubToken
    {
        public GitHubToken(string access_token, string scope, string token_type) =>
            (AccessToken, Scope, TokenType) = (access_token, scope, token_type);

        public static GitHubToken Empty { get; } = new GitHubToken(string.Empty, string.Empty, string.Empty);

        [JsonProperty("access_token")]
        public string AccessToken { get; }

        [JsonProperty("scope")]
        public string Scope { get; }

        [JsonProperty("token_type")]
        public string TokenType { get; }
    }

    public static class GitHubTokenExtensions
    {
        public static bool IsEmpty(this GitHubToken gitHubToken)
        {
            return gitHubToken.AccessToken == string.Empty
                    && gitHubToken.Scope == string.Empty
                    && gitHubToken.TokenType == string.Empty;
        }
    }
}
