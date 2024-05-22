using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record Contributor
{
	public Contributor(string login, Uri html_url, Uri avatar_url, long contributions, DateTimeOffset? dataDownloadedAt = null)
	{
		Login = login;
		GitHubUrl = html_url;
		AvatarUrl = avatar_url;
		ContributionCount = contributions;
		DataDownloadedAt = dataDownloadedAt ?? DateTimeOffset.UtcNow;
	}

	[JsonPropertyName("login")]
	public string Login { get; }

	[JsonPropertyName("avatar_url")]
	public Uri AvatarUrl { get; }

	[JsonPropertyName("html_url")]
	public Uri GitHubUrl { get; }

	[JsonPropertyName("contributions")]
	public long ContributionCount { get; }

	[JsonPropertyName("dataDownloadedAt")]
	public DateTimeOffset DataDownloadedAt { get; }
}