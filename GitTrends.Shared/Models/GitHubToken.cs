namespace GitTrends.Shared
{
    public class GitHubToken
    {
        public GitHubToken(string accessToken, string scope, string tokenType) =>
            (AccessToken, Scope, TokenType) = (accessToken, scope, tokenType);

        public string AccessToken { get; }
        public string Scope { get; }
        public string TokenType { get; }
    }
}
