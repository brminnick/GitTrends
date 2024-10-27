using System.Text.Json.Serialization;

namespace  GitTrends.Common;

public record Contributor(
	[property: JsonPropertyName("login")] string Login,
	[property: JsonPropertyName("avatar_url")] Uri AvatarUrl,
	[property: JsonPropertyName("html_url")] Uri GitHubUrl,
	[property: JsonPropertyName("contributions")] long ContributionCount,
	[property: JsonPropertyName("dataDownloadedAt")] DateTimeOffset? DataDownloadedAt = null);