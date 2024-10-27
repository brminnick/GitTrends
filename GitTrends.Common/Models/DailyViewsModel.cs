using System.Text.Json.Serialization;

namespace GitTrends.Common;

public record DailyViewsModel(
	[property: JsonPropertyName("timestamp")] DateTimeOffset Day,
	[property: JsonPropertyName("count")] long TotalCount,
	[property: JsonPropertyName("uniques")] long TotalUniqueCount)
	: IBaseDailyModel, IDailyViewsModel
{
	[JsonIgnore]
	public DateTime LocalDay => Day.LocalDateTime;

	[JsonIgnore]
	public long TotalViews => TotalCount;

	[JsonIgnore]
	public long TotalUniqueViews => TotalUniqueCount;
}