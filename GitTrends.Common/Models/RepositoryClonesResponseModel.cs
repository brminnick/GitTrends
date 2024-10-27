using System.Text.Json.Serialization;

namespace GitTrends.Common;

public record RepositoryClonesResponseModel(
	[property: JsonPropertyName("count")] long TotalCount,
	[property: JsonPropertyName("uniques")] long TotalUniqueCount,
	[property: JsonPropertyName("clones")] IReadOnlyList<DailyClonesModel> DailyClonesList);