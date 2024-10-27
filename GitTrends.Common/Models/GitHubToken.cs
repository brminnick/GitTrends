using System.Text.Json.Serialization;

namespace GitTrends.Common;

public record GitHubToken(
	[property: JsonPropertyName("access_token")] string AccessToken,
	[property: JsonPropertyName("scope")] string Scope,
	[property: JsonPropertyName("token_type")] string TokenType)
{
	public static GitHubToken Empty { get; } = new GitHubToken(string.Empty, string.Empty, string.Empty);
}

public static class GitHubTokenExtensions
{
	public static bool IsEmpty(this GitHubToken gitHubToken) => gitHubToken is { AccessToken: "", Scope: "", TokenType: "" };
}