using System.Text.Json.Serialization;

namespace GitTrends.Shared;

public record DailyClonesModel : BaseDailyModel, IDailyClonesModel
{
	public DailyClonesModel(DateTimeOffset timestamp, long count, long uniques) : base(timestamp, count, uniques)
	{

	}

	[JsonIgnore]
	public long TotalClones => TotalCount;

	[JsonIgnore]
	public long TotalUniqueClones => TotalUniqueCount;
}