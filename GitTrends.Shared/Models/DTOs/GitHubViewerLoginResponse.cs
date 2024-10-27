using System.Text.Json.Serialization;

namespace  GitTrends.Common;

public record GitHubViewerLoginResponse([property: JsonPropertyName("viewer")] User Viewer);