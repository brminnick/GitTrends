using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record GitHubViewerLoginResponse([property: JsonPropertyName("viewer")] User Viewer);