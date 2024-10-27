using System.Text.Json.Serialization;

namespace GitTrends.Common;

public record RepositoryViewsResponseModel(
	[property: JsonPropertyName("count")] long TotalCount,
	[property: JsonPropertyName("uniques")] long TotalUniqueCount,
	[property: JsonPropertyName("views")] IReadOnlyList<DailyViewsModel> DailyViewsList);