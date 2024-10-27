using System.Text.Json.Serialization;

namespace  GitTrends.Common;

public record DailyClonesModel(
	[property: JsonPropertyName("timestamp")] DateTimeOffset Day,
	[property: JsonPropertyName("count")] long TotalClones,
	[property: JsonPropertyName("uniques")] long TotalUniqueClones)
	: IBaseDailyModel, IDailyClonesModel
{
	[JsonIgnore]
	public DateTime LocalDay => Day.LocalDateTime;
	
	[JsonIgnore]
	long IBaseDailyModel.TotalCount => TotalClones;

	[JsonIgnore]
	long IBaseDailyModel.TotalUniqueCount => TotalUniqueClones;
}