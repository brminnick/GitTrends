using System.Text.Json.Serialization;
namespace  GitTrends.Common;

public abstract record BaseTotalCountModel(
	[property: JsonPropertyName("count")] long TotalCount, 
	[property: JsonPropertyName("uniques")] long TotalUniqueCount);