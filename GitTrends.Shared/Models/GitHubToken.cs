using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record GitHubToken
{
	public GitHubToken(string access_token, string scope, string token_type) =>
		(AccessToken, Scope, TokenType) = (access_token, scope, token_type);

	public static GitHubToken Empty { get; } = new GitHubToken(string.Empty, string.Empty, string.Empty);

	[JsonPropertyName("access_token")]
	public string AccessToken { get; }

	[JsonPropertyName("scope")]
	public string Scope { get; }

	[JsonPropertyName("token_type")]
	public string TokenType { get; }
}

public static class GitHubTokenExtensions
{
	public static bool IsEmpty(this GitHubToken gitHubToken) => gitHubToken.AccessToken == string.Empty
																&& gitHubToken.Scope == string.Empty
																&& gitHubToken.TokenType == string.Empty;
}